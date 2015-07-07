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
        private readonly IInternalPackageRepositoryFactory _factory;
        private readonly IStore _store;

        public PackageIndex(IInternalPackageRepositoryFactory factory, IStore store, int feedId)
        {
            _factory = factory;
            _store = store;
            _feedId = feedId;

            if (feedId <= 0)
            {
                throw new ArgumentException("Feed id is not valid.");
            }
        }

        public void AddPackage(IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion)
        {
            IInternalPackage localPackage;
            using (var transaction = _store.BeginTransaction())
            {
                localPackage = InternalPackage.Create(_feedId, package, isAbsoluteLatestVersion, isLatestVersion);
                transaction.Insert(localPackage);
                transaction.Commit();
            }

            _factory.AddFrameworkNames(localPackage.SupportedFrameworks);
        }

        public void UnlistPackage(IInternalPackage package)
        {
            IInternalPackage internalPackage = GetPackage(package.PackageId, package.GetSemanticVersion());
            if (internalPackage == null)
                return;

            internalPackage.Listed = false;

            using (var transaction = _store.BeginTransaction())
            {
                transaction.Update(internalPackage);
                transaction.Commit();
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

        public IEnumerable<IInternalPackage> GetVersions(ITransaction transaction, string packageId,
            bool allowPreRelease)
        {
            var query = transaction.Query<IInternalPackage>();

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

  



        protected virtual IInternalPackage LoadPackage(string id, string version)
        {
            using (var transaction = _store.BeginTransaction())
            {
                return
                    transaction.Query<IInternalPackage>()
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
                return transaction.Query<IInternalPackage>().Count();
            }
        }

        public void IncrementDownloadCount(IInternalPackage package)
        {
            using (var transaction = _store.BeginTransaction())
            {
                IEnumerable<IInternalPackage> packages = GetVersions(transaction, package.PackageId, true).ToList();

                var newestPackage = packages.Single(pk => pk.Id == package.Id);

                newestPackage.VersionDownloadCount++;

                foreach (var versionOfPackage in packages)
                {
                    versionOfPackage.DownloadCount = packages.Sum(pk => pk.VersionDownloadCount);

                    transaction.Update(versionOfPackage);
                }

                transaction.Commit();
            }
        }
    }
}