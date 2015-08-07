using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hangfire;
using Hangfire.Logging;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
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
        private readonly IFeedConfigurationService _feedConfigurationService;
        private readonly IPackageService _packageService;
        private readonly ILog _logger = LogProvider.For<ReindexPackagesForFeedJob>();

        public ReindexPackagesForFeedJob(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IFeedConfigurationService feedConfigurationService, IPackageService packageService) : base(store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _feedConfigurationService = feedConfigurationService;
            _packageService = packageService;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Execute(IJobCancellationToken cancellationToken, int feedId)
        {
            _logger.Info("Executing " + JobId + " job for feed id " + feedId);

            IFeedConfiguration config = _feedConfigurationService.FindByFeedId(feedId);

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

            cancellationToken.ThrowIfCancellationRequested();

            Dictionary<string, List<string>> packagesToKeep = new Dictionary<string, List<string>>();

            var packageDirectories = Directory.GetDirectories(packagesDirectory);

            IInternalPackageRepository packageRepository = _packageRepositoryFactory.Create(feedId);

            int packagesAdded = 0;
            int packagesDeleted = 0;

            foreach (string packageDirectory in packageDirectories)
            {
                var files = Directory.GetFiles(packageDirectory, "*.nupkg", SearchOption.TopDirectoryOnly);

                foreach (string file in files)
                {
                    IPackage package;

                    try
                    {
                        package = FastZipPackage.Open(file, new CryptoHashProvider());
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorException(
                            "Failed to open file " + file + " to check if it is indexed for feed id " + feedId, ex);
                        continue;
                    }

                    var existingPackage = packageRepository.GetPackage(package.Id, package.Version);

                    if (existingPackage != null)
                    {
                        AddToKeepDictionary(packagesToKeep, existingPackage.Id, existingPackage.Version);
                        continue;
                    }

                    _logger.Info("Indexing package " + package.Id + " v" + package.Version + " for feed id " + feedId);

                    packageRepository.IndexPackage(package);

                    AddToKeepDictionary(packagesToKeep, package.Id, package.Version.ToString());
                    packagesAdded++;
                }
            }

            var packages = _packageService.GetAllPackagesForFeed(feedId);

            foreach (var internalPackage in packages)
            {
                if (packagesToKeep.ContainsKey(internalPackage.Id))
                {
                    var versionList = packagesToKeep[internalPackage.Id];
                    if (versionList.Contains(internalPackage.Version))
                    {
                        continue;
                    }

                    _logger.Info("Deleting " + internalPackage.Id + ", v" + internalPackage.Version +
                                 " as the package file no longer exists for feed id " + feedId);

                    _packageService.Delete(internalPackage);
                    packagesDeleted++;
                }
                else
                {
                    _logger.Info("Deleting " + internalPackage.Id + ", v" + internalPackage.Version +
                                 " as the package file no longer exists for feed id " + feedId);

                    _packageService.Delete(internalPackage);
                    packagesDeleted++;
                }
            }


            _logger.Info("Finished package reindex for feed id " + feedId);
            _logger.Info("Removed " + packagesDeleted + " packages and added " + packagesAdded +
                         " packages for feed id " + feedId);
        }

        private void AddToKeepDictionary(Dictionary<string, List<string>> packagesToKeep, string packageId, string packageVersion)
        {
            if (packagesToKeep.ContainsKey(packageId))
            {
                packagesToKeep[packageId].Add(packageVersion);
            }
            else
            {
                packagesToKeep.Add(packageId, new List<string> {packageVersion});
            }
        }

        public string JobId => typeof (ReindexPackagesForFeedJob).Name;
    }
}