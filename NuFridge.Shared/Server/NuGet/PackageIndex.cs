using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Linq2Rest;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Model.Mappings;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.Nancy;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class PackageIndex
    {
        private readonly int _feedId;
        private readonly IStore _store;
       // private readonly ICurrentRequest _currentRequest;

        public PackageIndex(IStore store, int feedId)
        {
            _store = store;
            _feedId = feedId;
           // _currentRequest = currentRequest;

            if (feedId <= 0)
            {
                throw new ArgumentException("Feed id is not valid.");
            }
        }

        public void AddPackage(IInternalPackage package)
        {
     //       var username = _currentRequest.Context.CurrentUser.UserName;

            using (var dbContext = new WritableDatabaseContext(_store))
            {
                dbContext.Packages.Add((InternalPackage) package);
                dbContext.SaveChanges("admin");
            }
        }

        public void UnlistPackage(ITransaction transaction, IInternalPackage package)
        {
            IInternalPackage internalPackage = GetPackage(package.Id, package.GetSemanticVersion());
            if (internalPackage == null)
                return;

            internalPackage.Listed = false;

            transaction.Update(internalPackage);
        }

        public void DeletePackage(ITransaction transaction, IInternalPackage package)
        {
            IInternalPackage internalPackage = GetPackage(package.Id, package.GetSemanticVersion());
            if (internalPackage == null)
                return;

                transaction.Delete(internalPackage); 
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            return LoadPackage(packageId.ToLowerInvariant(), version.ToString().ToLowerInvariant());
        }

        public List<IInternalPackage> GetVersions(ITransaction transaction, string packageId,
            bool allowPreRelease)
        {
            var query = transaction.Query<IInternalPackage>();

            query.Where("PackageId LIKE @packageId AND FeedId = @feedId");
            query.Parameter("packageId", packageId).Parameter("feedId", _feedId);

            if (!allowPreRelease)
            {
                query.Where("[IsPrerelease] = 0");
            }

            var packages = query.ToList();

            return packages;
        }

  



        protected virtual IInternalPackage LoadPackage(string id, string version)
        {
            using (var transaction = _store.BeginTransaction())
            {
                return
                    transaction.Query<IInternalPackage>()
                        .Where("PackageId LIKE @packageId AND Version = @packageVersion AND FeedId = @feedId")
                        .Parameter("packageId", id)
                        .Parameter("packageVersion", version)
                        .Parameter("feedId", _feedId)
                        .First();
            }
        }

        public void IncrementDownloadCount(IInternalPackage package)
        {
            using (var transaction = _store.BeginTransaction())
            {
                IEnumerable<IInternalPackage> packages = GetVersions(transaction, package.Id, true).ToList();

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