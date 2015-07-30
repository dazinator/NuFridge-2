using System.IO;
using System.Linq;
using Hangfire;
using Hangfire.Logging;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.Actions.NuGetApiV2;
using NuGet;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("filesystem")]
    public class ReindexPackagesForFeedJob : PackagesBase
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;
        private readonly ILog _logger = LogProvider.For<ReindexPackagesForFeedJob>();

        public ReindexPackagesForFeedJob(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store) : base(store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
        }

        [AutomaticRetryAttribute(Attempts = 0)]
        public void Execute(IJobCancellationToken cancellationToken, int feedId)
        {
            _logger.Info("Executing " + JobId + " job for feed id " + feedId);

            cancellationToken.ThrowIfCancellationRequested();

            IFeedConfiguration config;
            using (var transaction = _store.BeginTransaction())
            {
                config =
                    transaction.Query<IFeedConfiguration>()
                        .Where("FeedId = @feedId")
                        .Parameter("feedId", feedId)
                        .First();
            }

            if (config == null)
            {
                _logger.Error("No feed configuration record could be found for feed id " + feedId);
                return;
            }


            if (string.IsNullOrWhiteSpace(config.Directory))
            {
                _logger.Error("No feed directory could be found for feed id " + feedId);
                return;
            }

            var packagesDirectory = config.PackagesDirectory;

            if (!Directory.Exists(packagesDirectory))
            {
                _logger.Error("No feed packages directory could be found for feed id " + feedId);
                return;
            }

            _logger.Info("Getting packages to remove from the index for feed id " + feedId);

            int packagesBeingDeleted = 0;

            using (var transaction = _store.BeginTransaction())
            {
                var packages = transaction.Query<IInternalPackage>().Where("FeedId = @feedId").Parameter("feedId", feedId).Stream();

                foreach (var feedPackage in packages)
                {
                    transaction.Delete(feedPackage);
                    packagesBeingDeleted++;
                }

                if (packagesBeingDeleted > 0)
                {
                    _logger.Warn("Removing " + packagesBeingDeleted + " packages from the index for feed id " + feedId);
                }

                cancellationToken.ThrowIfCancellationRequested();
                transaction.Commit();
            }

            var packageDirectories = Directory.GetDirectories(packagesDirectory);

            IInternalPackageRepository packageRepository = _packageRepositoryFactory.Create(feedId);

            int packagesReIndexed = 0;

            foreach (string packageDirectory in packageDirectories)
            {
                var files = Directory.GetFiles(packageDirectory, "*.nupkg", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    _logger.Debug("Opening " + file + " package to read for reindexing for feed id " + feedId);

                    IPackage package = FastZipPackage.Open(file, new CryptoHashProvider());

                    var existingPackage = packageRepository.GetPackage(package.Id, package.Version);

                    if (existingPackage != null)
                    {
                        _logger.Warn("Skipping file " + file + " on reindex for feed id " + feedId +
                                     ". The package already exists in the feed.");
                        continue;
                    }



                    bool isUploadedPackageLatestVersion;
                    bool isUploadedPackageAbsoluteLatestVersion;
                    UpdateLatestVersionFlagsForPackageId(package, packageRepository, out isUploadedPackageLatestVersion, out isUploadedPackageAbsoluteLatestVersion);

                    _logger.Info("Indexing package " + package.Id + " v" + package.Version + " for feed id " + feedId);
                    packageRepository.IndexPackage(package, isUploadedPackageAbsoluteLatestVersion, isUploadedPackageLatestVersion);

                    packagesReIndexed++;
                }
            }

            _logger.Info("Finished reindex for feed id " + feedId);
            _logger.Info("Removed " + packagesBeingDeleted + " packages and reindexed " + packagesReIndexed + " packages for feed id " + feedId);

            if (packagesReIndexed < packagesBeingDeleted)
            {
                _logger.Warn("The reindex for feed id " + feedId + " has finished with less packages than it originally had");
            }
        }

        public string JobId => typeof (ReindexPackagesForFeedJob).Name;
    }
}