using System;
using System.IO;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.NuGet.Packaging;
using NuFridge.Shared.Web.OData;
using NuGet;

namespace NuFridge.Shared.NuGet.Repository
{
    public class RemoteRemotePackageImporter : IRemotePackageImporter
    {
        private readonly IInternalPackageRepositoryFactory _factory;
        private readonly IPackageService _packageService;

        public RemoteRemotePackageImporter(IPackageService packageService, IInternalPackageRepositoryFactory factory)
        {
            _packageService = packageService;
            _factory = factory;
        }

        public void ImportPackage(int feedId, RemotePackageImportOptions options, ODataPackage package, PackageImportJobItem item)
        {
            IInternalPackageRepository localRepository = _factory.Create(feedId);
            IPackageRepository remoteRepository = PackageRepositoryFactory.Default.CreateRepository(options.FeedUrl);

            item.StartedAt = DateTime.Now;
            item.Log(LogLevel.Info, $"Searching for the package from {remoteRepository.Source}");

            var version = SemanticVersion.Parse(package.Version);

            DataServicePackage remotePackage = remoteRepository.FindPackage(package.Id, version, options.IncludePrerelease, false) as DataServicePackage;

            if (remotePackage == null)
            {
                throw new PackageNotFoundException(package.Id, package.Version);
            }

            if (options.CheckLocalCache)
            {
                item.Log(LogLevel.Debug,
                    "Searching local feeds for a NuGet package with the same hash " + remotePackage.PackageHash);

                if (TryImportFromLocalFeed(remotePackage, localRepository, item))
                    return;

                item.Log(LogLevel.Debug, "No matching package was found locally. The package will be downloaded from the remote feed");
            }
            else
            {
                item.Log(LogLevel.Info, "Downloading and indexing the package");
            }

            localRepository.AddPackage(remotePackage);

            item.Log(LogLevel.Info, "The package has been imported");
        }

        private bool TryImportFromLocalFeed(DataServicePackage remotePackage, IInternalPackageRepository localRepository, PackageImportJobItem item)
        {
            InternalPackage localVersionOfPackage = _packageService.GetPackage(null, item.PackageId, SemanticVersion.Parse(item.Version));

            if (!string.IsNullOrWhiteSpace(localVersionOfPackage?.Hash))
            {
                if (localVersionOfPackage.Hash == remotePackage.PackageHash)
                {
                    IInternalPackageRepository cachedRepo = _factory.Create(localVersionOfPackage.FeedId);

                    var cachePackagePath = cachedRepo.GetPackageFilePath(localVersionOfPackage);

                    if (File.Exists(cachePackagePath))
                    {
                        item.Log(LogLevel.Info, $"Found a matching package at {cachePackagePath}");

                        IFastZipPackage cachePackage = FastZipPackage.Open(cachePackagePath, new CryptoHashProvider());

                        cachePackage.Listed = remotePackage.IsListed();

                        localRepository.AddPackage(cachePackage);
                        return true;
                    }
                }
            }
             
            return false;
        }
    }

    public interface IRemotePackageImporter
    {
        void ImportPackage(int feedId, RemotePackageImportOptions options, ODataPackage package, PackageImportJobItem item);
    }
}