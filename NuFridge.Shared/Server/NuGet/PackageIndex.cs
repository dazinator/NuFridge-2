using System;
using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class PackageIndex
    {
        private readonly IPackageService _packageService;
        private readonly int _feedId;

        public PackageIndex(IPackageService packageService, int feedId)
        {
            _packageService = packageService;
            _feedId = feedId;

            if (feedId <= 0)
            {
                throw new ArgumentException("Feed id is not valid.");
            }
        }

        public void AddPackage(IInternalPackage package)
        {
            _packageService.Insert((InternalPackage)package);
        }

        public void UnlistPackage(IInternalPackage package)
        {
            package.Listed = false;

            _packageService.Update((InternalPackage)package);
        }

        public void DeletePackage(IInternalPackage package)
        {
            _packageService.Delete((InternalPackage)package);
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            return _packageService.GetPackage(_feedId, packageId, version.ToString());
        }

        public void IncrementDownloadCount(IInternalPackage package)
        {
            using (var dbContext = new DatabaseContext())
            {
                IEnumerable<IInternalPackage> packages =
                    EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + _feedId).Where(
                        pk =>
                            pk.Id.Equals(package.Id, StringComparison.InvariantCultureIgnoreCase) &&
                            pk.FeedId == _feedId);

                var newestPackage = packages.Single(pk => pk.PrimaryId == package.PrimaryId);

                newestPackage.VersionDownloadCount++;

                foreach (var versionOfPackage in packages)
                {
                    versionOfPackage.DownloadCount = packages.Sum(pk => pk.VersionDownloadCount);
                }

                dbContext.SaveChanges();
            }
        }
    }
}