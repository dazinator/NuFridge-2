using System;
using System.IO;
using System.Linq;
using Hangfire;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet.Import
{
    public class PackageImporter : IPackageImporter
    {
        private readonly IInternalPackageRepositoryFactory _factory;
        private readonly IPackageService _packageService;
        private readonly ILog _log = LogProvider.For<PackageImporter>();


        public PackageImporter(IPackageService packageService, IInternalPackageRepositoryFactory factory)
        {
            _packageService = packageService;
            _factory = factory;
        }


        public void ImportPackage(int feedId, PackageImportOptions options, PackageImportJobItem item)
        {
            SemanticVersion version = new SemanticVersion(item.Version);

            IInternalPackageRepository localRepository = _factory.Create(feedId);

            IPackageRepository remoteRepository = PackageRepositoryFactory.Default.CreateRepository(options.FeedUrl);

            DataServicePackage remotePackage =
                remoteRepository.GetPackages()
                    .Where(pk => pk.Id == item.PackageId)
                    .ToList()
                    .FirstOrDefault(pk => pk.Version == version) as DataServicePackage;

            if (remotePackage == null)
            {
                throw new PackageNotFoundException();
            }

            if (options.CheckLocalCache)
            {
                if (TryImportFromLocalFeed(remotePackage, localRepository, item))
                    return;
            }

            localRepository.AddPackage(remotePackage);
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
                        IFastZipPackage cachePackage = FastZipPackage.FastZipPackage.Open(cachePackagePath, new CryptoHashProvider());

                        cachePackage.Listed = remotePackage.IsListed();

                        localRepository.AddPackage(cachePackage);
                        return true;
                    }
                }
            }
             
            return false;
        }
    }

    public interface IPackageImporter
    {
        void ImportPackage(int feedId, PackageImportOptions options, PackageImportJobItem item);
    }
}