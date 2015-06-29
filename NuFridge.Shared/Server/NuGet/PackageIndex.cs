using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Linq2Rest;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class PackageIndex
    {
        private readonly int _feedId;
        private readonly IStore _store;

        public PackageIndex(IStore store, int feedId)
        {
            _store = store;
            _feedId = feedId;

            if (feedId <= 0)
            {
                throw new ArgumentException("Feed id is not valid.");
            }
        }

        public bool DoesPackageExist(IInternalPackage package)
        {
            return LoadPackage(package) != null;
        }

        public void AddPackage(IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion)
        {
            using (var transaction = _store.BeginTransaction())
            {
                var localPackage = InternalPackage.Create(_feedId, package, isAbsoluteLatestVersion, isLatestVersion);
                transaction.Insert(localPackage);
                transaction.Commit();
            }
        }

        public IQueryable<IInternalPackage> GetPackages()
        {
            using (var transaction = _store.BeginTransaction())
            {
                return transaction.Query<InternalPackage>().Stream().AsQueryable();
            }
        }

        public void DeletePackage(IInternalPackage package)
        {
            IInternalPackage internalPackage = GetPackage(package.PackageId, package.GetSemanticVersion());
            if (internalPackage == null)
                return;
            using (var transaction = _store.BeginTransaction())
            {
                transaction.Delete(internalPackage);
                transaction.Commit();
            }
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            return LoadPackage(packageId.ToLowerInvariant(), version.ToString().ToLowerInvariant());
        }

        public IInternalPackage GetLatest(string packageId, bool allowPreRelease = true)
        {
            int totalResults;
            return CallProcedure(out totalResults, packageId, allowPreRelease, false, true).FirstOrDefault();
        }

        public IEnumerable<IInternalPackage> GetVersions(ITransaction transaction, string packageId, bool allowPreRelease)
        {
                var query = transaction.Query<InternalPackage>();

                query.Where("FeedId = @feedId");
                query.Parameter("feedId", _feedId);

                query.Where("PackageId = @packageId");
                query.Parameter("packageId", packageId);

                if (!allowPreRelease)
                {
                    query.Where("[IsPrerelease] = 0");
                }

                var packages = query.Stream();

                return packages;
        }

        public IEnumerable<IInternalPackage> GetWebPackages(ITransaction transaction, string filterType, string filterColumn, string filterValue, string orderType, string orderProperty, string searchTerm, string targetFramework, string includePrerelease)
        {

                var query = transaction.Query<InternalPackage>();

                if (filterType == "eq")
                {
                    dynamic value = filterValue;

                    if (value.ToLower() == "true")
                    {
                        value = 1;
                    }
                    else if (value.ToLower() == "false")
                    {
                        value = 0;
                    }

                    query.Where(string.Format("[{0}] = @filterValue", filterColumn));
                    query.Parameter("filterValue", value);
                }
                else if (filterType == "not")
                {
                    query.Where(string.Format("[{0}] = @filterValue", filterValue));
                    query.Parameter("filterValue", 0);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query.Where("[PackageId] LIKE @packageIdSearch");
                    query.LikeParameter("packageIdSearch", searchTerm.Substring(1, searchTerm.Length - 2));
                }

                if (!string.IsNullOrWhiteSpace(targetFramework))
                {
                    //TODO
                }

                if (!string.IsNullOrWhiteSpace(includePrerelease))
                {
                    if (includePrerelease.ToLower() == "false")
                    {
                        query.Where("[IsPrerelease] = @includePrerelease");
                        query.Parameter("includePrerelease", 0);
                    }
                }

                if (!string.IsNullOrWhiteSpace(orderType) && !string.IsNullOrWhiteSpace(orderProperty))
                {
                    var prop = orderProperty;

                    //Id is the PK not the package id
                    if (prop.ToLower() == "id")
                    {
                        prop = "PackageId";
                    }

                    //TODO check how secure this is as there are no parameters involved
                    query.OrderBy(String.Format("[{0}] {1}", prop, orderType));
                }

                query.Where("FeedId = @feedId");
                query.Parameter("feedId", _feedId);



                var packages = query.Stream();



                return packages;
            
        }

        public List<IInternalPackage> GetPackagesContaining(string searchTerm, out int total, int skip = 0, int take = 30, bool allowPreRelease = true)
        {
            bool flag1 = allowPreRelease;
            bool flag2 = true;
            int num1 = skip;
            int num2 = take;
            bool flag3 = true;
            string packageId = searchTerm;
            int num3 = flag1 ? 1 : 0;
            int num4 = flag3 ? 1 : 0;
            int num5 = flag2 ? 1 : 0;
            int skip1 = num1;
            int take1 = num2;
            return CallProcedure(out total, packageId, num3 != 0, num4 != 0, num5 != 0, skip1, take1);
        }

        public List<IInternalPackage> GetLatestOfAllPackages(out int total, int skip = 0, int take = 30, bool allowPreRelease = true)
        {
            return CallProcedure(out total, null, allowPreRelease, false, true, skip, take);
        }

        private List<IInternalPackage> CallProcedure(out int totalResults, string packageId = null, bool allowPreRelease = true, bool partialMatch = false, bool latestOnly = false, int skip = 0, int take = 30)
        {
            using (var transaction = _store.BeginTransaction())
            {
                int total = 0;
                CommandParameters commandParameters = new CommandParameters();
                commandParameters.Add("feedId", _feedId);
                commandParameters.Add("allowPreRelease", allowPreRelease ? 1 : 0);
                commandParameters.Add("packageId", packageId ?? string.Empty);
                commandParameters.Add("latestOnly", latestOnly ? 1 : 0);
                commandParameters.Add("minRow", skip + 1);
                commandParameters.Add("maxRow", skip + take);
                commandParameters.Add("partialMatch", partialMatch ? 1 : 0);
                CommandParameters args = commandParameters;
                args.CommandType = CommandType.StoredProcedure;
                IEnumerable<InternalPackage> enumerable = transaction.ExecuteReaderWithProjection("GetPackages", args,
                    mapper =>
                    {
                        InternalPackage internalPackage = mapper.Map<InternalPackage>("");
                        if (total == 0)
                            mapper.Read(reader => total = (int) reader["TotalCount"]);
                        return internalPackage;
                    });
                totalResults = total;
                return enumerable.Cast<IInternalPackage>().ToList();
            }
        }

        private InternalPackage LoadPackage(IInternalPackage package)
        {
            return LoadPackage(package.PackageId.ToLowerInvariant(), package.Version.ToString().ToLowerInvariant());
        }

        protected virtual InternalPackage LoadPackage(string id, string version)
        {
            using (var transaction = _store.BeginTransaction())
            {
                return
                    transaction.Query<InternalPackage>()
                        .Where("PackageId = @packageId AND Version = @packageVersion AND FeedId = @feedId")
                        .Parameter("packageId", id)
                        .Parameter("packageVersion", version)
                        .Parameter("feedId", _feedId)
                        .First();
            }
        }

        public int GetCount()
        {
            using (var transaction = _store.BeginTransaction())
            {
                return transaction.Query<InternalPackage>().Count();
            }
        }

        public void IncrementDownloadCount(IInternalPackage package)
        {
            package.DownloadCount++;

            using (var transaction = _store.BeginTransaction())
            {
                transaction.Update(package);
                transaction.Commit();
            }
        }
    }
}
