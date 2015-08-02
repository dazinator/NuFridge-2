using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Linq2Rest;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Model.Mappings;
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

        public void AddPackage(ITransaction transaction, IInternalPackage package)
        {
            transaction.Insert(InternalPackageMap.GetPackageTable(_feedId), package);
        }

        public void UnlistPackage(ITransaction transaction, IInternalPackage package)
        {
            IInternalPackage internalPackage = GetPackage(package.Id, package.GetSemanticVersion());
            if (internalPackage == null)
                return;

            internalPackage.IsAbsoluteLatestVersion = false;
            internalPackage.IsLatestVersion = false;
            internalPackage.Listed = false;

            transaction.Update(InternalPackageMap.GetPackageTable(_feedId), internalPackage);
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

            query.Where(_feedId);

            query.Where("PackageId LIKE @packageId");
            query.Parameter("packageId", packageId);

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
                        .Where(_feedId)
                        .Where("PackageId LIKE @packageId AND Version = @packageVersion")
                        .Parameter("packageId", id)
                        .Parameter("packageVersion", version)
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

                    transaction.Update(InternalPackageMap.GetPackageTable(_feedId), versionOfPackage);
                }

                transaction.Commit();
            }
        }
    }
}