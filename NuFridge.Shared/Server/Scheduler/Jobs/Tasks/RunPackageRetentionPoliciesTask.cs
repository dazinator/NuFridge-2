using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentScheduler;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs.Tasks
{
    public class RunPackageRetentionPoliciesTask : ITask
    {
        private IStore Store { get; set; }
        private InternalPackageRepositoryFactory PackageRepositoryFactory { get; set; }
        private readonly ILog _log = LogProvider.For<RunPackageRetentionPoliciesTask>();

        public RunPackageRetentionPoliciesTask(IStore store, InternalPackageRepositoryFactory packageRepositoryFactory)
        {
            Store = store;
            PackageRepositoryFactory = packageRepositoryFactory;
        }

        public void Execute()
        {
            _log.Info("Running package retention policies.");

            List<IFeed> feeds;
            List<IFeedConfiguration> configs;

            using (var transaction = Store.BeginTransaction())
            {
                feeds = transaction.Query<IFeed>().ToList();
                configs = transaction.Query<IFeedConfiguration>().ToList();
            }

            var feedDictionary = new Dictionary<IFeed, IFeedConfiguration>();

            foreach (var feed in feeds)
            {
                var config = configs.FirstOrDefault(cf => cf.FeedId == feed.Id);
                if (config != null)
                {
                    feedDictionary.Add(feed, config);
                }
            }

            RunPolicies(feedDictionary);

            _log.Info("Finished running package retention policies.");
        }

        private void RunPolicies(Dictionary<IFeed, IFeedConfiguration> feedDictionary)
        {
            foreach (var feedKvp in feedDictionary)
            {
                RunPolicy(feedKvp.Key, feedKvp.Value);
            }
        }

        protected virtual bool IsPackageDirectoryValid(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return Directory.Exists(path);
        }

        private void RunPolicy(IFeed feed, IFeedConfiguration config)
        {
            var directory = config.PackagesDirectory;

            if (!config.RetentionPolicyEnabled)
            {
                _log.Debug(string.Format("The '{0}' feed does not use a package retention policy, no packages will be deleted.", feed.Name));
                return;
            }

            if (!IsPackageDirectoryValid(directory))
            {
                _log.Warn(String.Format("The package directory for the '{0}' feed does not exist. This can happen when no packages have been uploaded to the feed yet.", feed.Name));
                return;
            }

            _log.Info(string.Format("Running package retention policy for '{0}'. Max release packages: {1}, Max prerelease packages: {2}.", feed.Name, config.MaxReleasePackages, config.MaxPrereleasePackages));

            List<IInternalPackage> packages;

            using (var transaction = Store.BeginTransaction())
            {
                packages = transaction.Query<IInternalPackage>().Where("FeedId = @feedId").Parameter("feedId", feed.Id).ToList();
            }

            Dictionary<string, List<IInternalPackage>> packagesGroupedById = packages.GroupBy(x => x.PackageId).ToDictionary(x => x.Key, x => x.ToList());

            int releasePackagesDeleted = 0;
            int prereleasePackagesDeleted = 0;

            foreach (var packageGroupList in packagesGroupedById)
            {
                var releasePackages = packageGroupList.Value.Where(pk => !pk.IsPrerelease).ToList();
                var prereleasePackages = packageGroupList.Value.Where(pk => pk.IsPrerelease).ToList();

                if (releasePackages.Count() > config.MaxReleasePackages)
                {
                    releasePackagesDeleted += FindAndRemoveOldReleasePackages(config, releasePackages);
                }
                if (prereleasePackages.Count() > config.MaxPrereleasePackages)
                {
                    prereleasePackagesDeleted += FindAndRemoveOldPrereleasePackages(config, prereleasePackages);
                }
            }

            _log.Info(string.Format("Finished package retention policy for '{0}'. {1} release packages deleted. {2} prerelease packages deleted.", feed.Name, releasePackagesDeleted, prereleasePackagesDeleted));
        }

        private int FindAndRemoveOldReleasePackages(IFeedConfiguration config, List<IInternalPackage> packages)
        {
            packages.Sort((a, b) => a.GetSemanticVersion().CompareTo(b.GetSemanticVersion()));

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
                _log.Info(string.Format("Deleting {0} release versions of the '{1}' package.", toDeleteCount, packages.First().PackageId));
                var packageRepo = PackageRepositoryFactory.Create(config.FeedId);

                var toDeletePackages = Enumerable.Reverse(packages).Take(toDeleteCount);

                foreach (var packageToDelete in toDeletePackages)
                {
                    try
                    {
                        packageRepo.RemovePackage(packageToDelete);
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorException(string.Format("There was an error trying to remove the '{0}' package with version '{1}' for the retention policy.", packageToDelete.PackageId, packageToDelete.Version), ex);
                    }
                }
            }

            return toDeleteCount > 0 ? toDeleteCount : 0;
        }

        private int FindAndRemoveOldPrereleasePackages(IFeedConfiguration config, List<IInternalPackage> packages)
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
                _log.Info(string.Format("Deleting {0} prerelease versions of the '{1}' package.", toDeleteCount, packages.First().PackageId));
                var packageRepo = PackageRepositoryFactory.Create(config.FeedId);

                var toDeletePackages = Enumerable.Reverse(packages).Take(toDeleteCount);

                foreach (var packageToDelete in toDeletePackages)
                {
                    try
                    {
                        packageRepo.RemovePackage(packageToDelete);
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorException(string.Format("There was an error trying to remove the '{0}' package with version '{1}' for the retention policy.", packageToDelete.PackageId, packageToDelete.Version), ex);
                    }
                }
            }

            return toDeleteCount > 0 ? toDeleteCount : 0;
        }
    }
}