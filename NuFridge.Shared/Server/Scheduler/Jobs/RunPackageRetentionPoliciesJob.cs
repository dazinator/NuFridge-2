using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hangfire;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("filesystem")]
    public class RunPackageRetentionPoliciesJob : JobBase
    {
        private readonly IFeedService _feedService;
        private readonly IFeedConfigurationService _feedConfigurationService;
        private readonly IPackageService _packageService;
        private IStore Store { get; set; }
        private IInternalPackageRepositoryFactory PackageRepositoryFactory { get; }
        private readonly ILog _log = LogProvider.For<RunPackageRetentionPoliciesJob>();

        public override string JobId => typeof(RunPackageRetentionPoliciesJob).Name;

        public override bool TriggerOnRegister => false;
        public override string Cron => "0 0 * * *"; //Every day at 00:00

        public RunPackageRetentionPoliciesJob(IStore store, IInternalPackageRepositoryFactory packageRepositoryFactory, IFeedService feedService, IFeedConfigurationService feedConfigurationService, IPackageService packageService)
        {
            _feedService = feedService;
            _feedConfigurationService = feedConfigurationService;
            _packageService = packageService;
            Store = store;
            PackageRepositoryFactory = packageRepositoryFactory;
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _log.Info("Running package retention policies.");

            List<Feed> feeds = _feedService.GetAll().ToList();

            RunPolicies(feeds, cancellationToken);

            _log.Info("Finished running package retention policies.");
        }

        private void RunPolicies(List<Feed> feeds, IJobCancellationToken cancellationToken)
        {
            foreach (var feed in feeds)
            {
                RunPolicy(feed, cancellationToken);
            }
        }

        protected virtual bool IsPackageDirectoryValid(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return Directory.Exists(path);
        }

        private void RunPolicy(IFeed feed, IJobCancellationToken cancellationToken)
        {
            var config = _feedConfigurationService.FindByFeedId(feed.Id);

            if (config == null)
            {
                return;
            }

            var directory = config.PackagesDirectory;

            if (!config.RetentionPolicyEnabled)
            {
                _log.Debug(
                    $"The '{feed.Name}' feed does not use a package retention policy, no packages will be deleted.");
                return;
            }

            if (config.MaxPrereleasePackages < 0)
            {
                config.MaxPrereleasePackages = 0;
            }

            if (config.MaxReleasePackages < 0)
            {
                config.MaxReleasePackages = 0;
            }

            if (config.MaxPrereleasePackages == 0 && config.MaxReleasePackages == 0)
            {
                _log.Debug(
                    $"The '{feed.Name}' feed is set to not keep any packages, the retention policy will not be run.");
                return;
            }

            if (!IsPackageDirectoryValid(directory))
            {
                _log.Warn(
                    $"The package directory for the '{feed.Name}' feed does not exist. This can happen when no packages have been uploaded to the feed yet.");
                return;
            }

            _log.Info(
                $"Running package retention policy for '{feed.Name}'. Max release packages: {config.MaxReleasePackages}, Max prerelease packages: {config.MaxPrereleasePackages}.");

            cancellationToken.ThrowIfCancellationRequested();

            List<InternalPackage> packages = _packageService.GetAllPackagesForFeed(feed.Id).ToList();

            Dictionary<string, List<InternalPackage>> packagesGroupedById = packages.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList());

            int releasePackagesDeleted = 0;
            int prereleasePackagesDeleted = 0;

            foreach (var packageGroupList in packagesGroupedById)
            {
                var releasePackages = packageGroupList.Value.Where(pk => !pk.IsPrerelease).ToList();
                var prereleasePackages = packageGroupList.Value.Where(pk => pk.IsPrerelease).ToList();

                if (releasePackages.Count() > config.MaxReleasePackages)
                {
                    releasePackagesDeleted += FindAndRemoveOldReleasePackages(config, releasePackages, cancellationToken);
                }
                if (prereleasePackages.Count() > config.MaxPrereleasePackages)
                {
                    prereleasePackagesDeleted += FindAndRemoveOldPrereleasePackages(config, prereleasePackages, cancellationToken);
                }
            }

            _log.Info(
                $"Finished package retention policy for '{feed.Name}'. {releasePackagesDeleted} release packages deleted. {prereleasePackagesDeleted} prerelease packages deleted.");
        }

        private int FindAndRemoveOldReleasePackages(IFeedConfiguration config, List<InternalPackage> packages, IJobCancellationToken cancellationToken)
        {
            packages.Sort((a, b) => b.GetSemanticVersion().CompareTo(a.GetSemanticVersion()));

            var packageCount = packages.Count();
            for (int i = packageCount; i-- > 0; )
            {
                var package = packages[i];

                //We never want to remove the latest version unless 0 is specified as the max
                if (package.IsLatestVersion && config.MaxReleasePackages > 0)
                {
                    packages.RemoveAt(i);
                }
            }

            int toDeleteCount = packages.Count() - config.MaxReleasePackages;

            if (toDeleteCount > 0)
            {
                _log.Info($"Deleting {toDeleteCount} release versions of the '{packages.First().Id}' package.");
                var packageRepo = PackageRepositoryFactory.Create(config.FeedId);

                var toDeletePackages = Enumerable.Reverse(packages).Take(toDeleteCount);

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var packageToDelete in toDeletePackages)
                {
                    try
                    {
                        if (config.RetentionPolicyDeletePackages)
                        {
                            packageRepo.DeletePackage(packageToDelete);
                        }
                        else
                        {
                            packageRepo.RemovePackage(packageToDelete);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorException(
                            $"There was an error trying to remove the '{packageToDelete.Id}' package with version '{packageToDelete.Version}' for the retention policy.", ex);
                    }
                }
            }

            return toDeleteCount > 0 ? toDeleteCount : 0;
        }

       

        private int FindAndRemoveOldPrereleasePackages(IFeedConfiguration config, List<InternalPackage> packages, IJobCancellationToken cancellationToken)
        {
            packages.Sort((a, b) => a.GetSemanticVersion().CompareTo(b.GetSemanticVersion()));

            var packageCount = packages.Count();
            for (int i = packageCount; i-- > 0; )
            {
                var package = packages[i];

                //We never want to remove the latest absolute version unless 0 is specified as the max
                if (package.IsAbsoluteLatestVersion && config.MaxPrereleasePackages > 0)
                {
                    packages.RemoveAt(i);
                }
            }

            int toDeleteCount = packages.Count() - config.MaxPrereleasePackages;

            if (toDeleteCount > 0)
            {
                _log.Info($"Deleting {toDeleteCount} prerelease versions of the '{packages.First().Id}' package.");
                var packageRepo = PackageRepositoryFactory.Create(config.FeedId);

                var toDeletePackages = Enumerable.Reverse(packages).Take(toDeleteCount);

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var packageToDelete in toDeletePackages)
                {
                    try
                    {
                        if (config.RetentionPolicyDeletePackages)
                        {
                            packageRepo.DeletePackage(packageToDelete);
                        }
                        else
                        {
                            packageRepo.RemovePackage(packageToDelete);
                        }
        
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorException(
                            $"There was an error trying to remove the '{packageToDelete.Id}' package with version '{packageToDelete.Version}' for the retention policy.", ex);
                    }
                }
            }

            return toDeleteCount > 0 ? toDeleteCount : 0;
        }
    }
}