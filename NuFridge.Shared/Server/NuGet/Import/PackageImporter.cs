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
    [Queue("filesystem")]
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

        [AutomaticRetry(Attempts = PackageImportProgressTracker.RetryCount)]
        public void ImportPackage(string parentJobId, int feedId, string feedUrl, string packageId, string strVersion, bool useLocalPackages)
        {
            SemanticVersion version = new SemanticVersion(strVersion);
            strVersion = version.ToString();

            try
            {
                _log.Debug("Beginning import of package " + packageId + " v" + strVersion + " to feed id " + feedId);

                IInternalPackageRepository localRepository = _factory.Create(feedId);

                IPackageRepository remoteRepository = PackageRepositoryFactory.Default.CreateRepository(feedUrl);

                DataServicePackage remotePackage =
                    remoteRepository.GetPackages()
                        .Where(pk => pk.Id == packageId)
                        .ToList()
                        .FirstOrDefault(pk => pk.Version == version) as DataServicePackage;

                if (remotePackage == null)
                {
                    _log.Warn("This package was not found on the remote NuGet feed. Id = " + packageId + ", Version = " +
                              version);
                    throw new ImportPackageException("This package was not found on the remote NuGet feed.");
                }

                if (useLocalPackages)
                {
                    if (TryImportFromLocalFeed(parentJobId, feedId, packageId, remotePackage, localRepository, version))
                        return;
                }

                localRepository.AddPackage(remotePackage);

                _log.Info("Completed import of package " + packageId + " v" + strVersion + " to feed id " + feedId);

                PackageImportProgressTracker.Instance.IncrementSuccessCount(parentJobId,
                    new PackageImportProgressAuditItem(packageId, version.ToString()));
            }
            catch (PackageConflictException ex)
            {
                _log.Info(ex.Message);

                var auditItem = new PackageImportProgressAuditItem(packageId, version.ToString(), ex.Message);

                PackageImportProgressTracker.Instance.IncrementFailureCount(parentJobId, auditItem);
                //Do not throw an exception as this will trigger a retry of the job
            }
            catch (InvalidPackageMetadataException ex)
            {
                _log.ErrorException(ex.Message, ex);

                var auditItem = new PackageImportProgressAuditItem(packageId, version.ToString(), ex.Message);

                PackageImportProgressTracker.Instance.IncrementFailureCount(parentJobId, auditItem);
                //Do not throw an exception as this will trigger a retry of the job
            }
            catch (Exception ex)
            {
                var message = "There was an error importing the " + packageId + " package v" + strVersion + " to the feed " + feedId + ".";

                _log.ErrorException(message, ex);

                var auditItem = new PackageImportProgressAuditItem(packageId, version.ToString(), ex.Message);

                PackageImportProgressTracker.Instance.IncrementFailureCount(parentJobId, auditItem);

                //Throw an exception so it will trigger a retry of the job
                throw new ImportPackageException(ex.Message);
            }
        }

        private bool TryImportFromLocalFeed(string parentJobId, int feedId, string packageId, DataServicePackage remotePackage, IInternalPackageRepository localRepository, SemanticVersion version)
        {
            InternalPackage localVersionOfPackage = _packageService.GetPackage(null, packageId, version);

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

                        _log.Info("Completed import of package " + packageId + " v" + version + " to feed id " + feedId + " using cached package from feed id " + localVersionOfPackage.FeedId);

                        PackageImportProgressTracker.Instance.IncrementSuccessCount(parentJobId, new PackageImportProgressAuditItem(packageId, version.ToString()));
                        return true;
                    }
                }
            }
             
            return false;
        }
    }

    public interface IPackageImporter
    {
        void ImportPackage(string parentJobId, int feedId, string feedUrl, string packageId, string strVersion, bool useLocalPackages);
    }
}