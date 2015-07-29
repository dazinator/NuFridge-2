using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Hangfire;
using Microsoft.AspNet.SignalR;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.Actions.NuGetApiV2;
using NuFridge.Shared.Server.Web.SignalR;
using NuGet;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class ImportPackagesForFeedJob : PackagesBase
    {
        private readonly IInternalPackageRepositoryFactory _factory;
        private readonly ILog _log = LogProvider.For<ImportPackagesForFeedJob>();

        public ImportPackagesForFeedJob(IInternalPackageRepositoryFactory factory, IStore store) : base(store)
        {
            _factory = factory;
        }

        public void Execute(IJobCancellationToken cancellationToken, int feedId, FeedImportOptions options)
        {
            _log.Info("Running import packages job for feed id " + feedId);

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ImportPackagesHub>();
            IPackageRepository remoteRepository = PackageRepositoryFactory.Default.CreateRepository(options.FeedUrl);
            IInternalPackageRepository localRepository = _factory.Create(feedId);

            List<IPackage> packages = GetPackages(remoteRepository, options);

            _log.Info("Found " + packages.Count + " for import for feed id " + feedId + " from " + options.FeedUrl);

            FeedImportStatus importStatus = new FeedImportStatus(feedId, packages.Count());

            hubContext.Clients.Group(ImportPackagesHub.GetGroup(feedId)).importPackagesUpdate(importStatus);

            ProcessPackages(hubContext, localRepository, packages, ref importStatus);

            _log.Info("Sending final update to subscribed clients for feed import (" + feedId + ").");
            hubContext.Clients.Group(ImportPackagesHub.GetGroup(feedId)).importPackagesUpdate(importStatus);
        }

        private void ProcessPackages(IHubContext hubContext, IInternalPackageRepository localRepository, List<IPackage> packages, ref FeedImportStatus importStatus)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            List< FeedImportStatus.FailedPackageItem> failedPackages = new List<FeedImportStatus.FailedPackageItem>();
            List<FeedImportStatus.PackageItem> completedPackages = new List<FeedImportStatus.PackageItem>();

            foreach (var package in packages)
            {
                var milliSeconds = watch.ElapsedMilliseconds;
                if (milliSeconds >= 5000)
                {
                    _log.Info($"{importStatus.CompletedCount} of {importStatus.TotalCount} packages have been imported for feed id {localRepository.FeedId}");
                    hubContext.Clients.Group(ImportPackagesHub.GetGroup(localRepository.FeedId)).importPackagesUpdate(importStatus);
                    watch.Restart();
                }

                var existingPackage = localRepository.GetPackage(package.Id, package.Version);

                if (existingPackage != null)
                {
                    _log.Warn("A package with the same ID and version already exists. Overwriting packages is not enabled on this feed. Id = " + package.Id + ", Version = " + package.Version);
                    importStatus.FailedCount++;
                    importStatus.RemainingCount--;
                    failedPackages.Add(new FeedImportStatus.FailedPackageItem {PackageId = package.Id, Version = package.Version.ToString(), Error = "A package with the same ID and version already exists." });
                    continue;
                }

                bool isUploadedPackageLatestVersion;
                bool isUploadedPackageAbsoluteLatestVersion;
                UpdateLatestVersionFlagsForPackageId(package, localRepository, out isUploadedPackageLatestVersion, out isUploadedPackageAbsoluteLatestVersion);

                try
                {
                    localRepository.AddPackage(package, isUploadedPackageAbsoluteLatestVersion,
                        isUploadedPackageLatestVersion);
                }
                catch (Exception ex)
                {
                    _log.ErrorException("There was an error importing the " + package.Id + " package v" + package.Version +" to the feed " + localRepository.FeedId + ". " + ex.Message, ex);
                    importStatus.FailedCount++;
                    importStatus.RemainingCount--;
                    failedPackages.Add(new FeedImportStatus.FailedPackageItem { PackageId = package.Id, Version = package.Version.ToString(), Error = ex.Message });
                    continue;
                }

                importStatus.CompletedCount++;
                importStatus.RemainingCount--;
                completedPackages.Add(new FeedImportStatus.PackageItem { PackageId = package.Id, Version = package.Version.ToString()});
            }

            watch.Stop();

            importStatus.IsCompleted = true;
            importStatus.SuccessfulImports = completedPackages;
            importStatus.FailedImports = failedPackages;
        }

        private List<IPackage> FilterByVersionSelector(List<IPackage> packages, FeedImportOptions options)
        {
            List<IPackage> filtered = new List<IPackage>();

            if (!packages.Any())
            {
                return filtered;
            }

            Func<List<IPackage>, IPackage> getLatestReleasePackage = delegate (List<IPackage> groupedPackages)
            {
                var releasePackages = groupedPackages.Where(pk => pk.IsReleaseVersion()).ToList();
                if (!releasePackages.Any())
                {
                    return null;
                }
                var latestVersionPackage = releasePackages.Aggregate(releasePackages[0], (highest, candiate) => candiate.Version.CompareTo(highest.Version) > 0 ? candiate : highest);
                return latestVersionPackage;
            };

            Func<List<IPackage>, IPackage> getLatestPrereleasePackage = delegate(List<IPackage> groupedPackages)
            {
                var prereleasePackages = groupedPackages.Where(pk => !pk.IsReleaseVersion()).ToList();
                if (!prereleasePackages.Any())
                {
                    return null;
                }
                var latestAbsoluteVersionPackage = prereleasePackages.Aggregate(prereleasePackages[0], (highest, candiate) => candiate.Version.CompareTo(highest.Version) > 0 ? candiate : highest);
                return latestAbsoluteVersionPackage;
            };


            Lazy<Dictionary<string, List<IPackage>>> packagesGroupedById =
                new Lazy<Dictionary<string, List<IPackage>>>(delegate
                {
                    return packages.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList());
                });


            switch (options.VersionSelector.Value)
            {
                case FeedImportOptions.VersionSelectorEnum.AllVersions:
                    filtered = packages;
                    break;
                case FeedImportOptions.VersionSelectorEnum.LatestReleaseAndPrereleaseVersion:

                    foreach (var packagesGroupedKvp in packagesGroupedById.Value)
                    {
                        var latestVersionPackage = getLatestReleasePackage(packagesGroupedKvp.Value);
                        var latestAbsoluteVersionPackage = getLatestPrereleasePackage(packagesGroupedKvp.Value);

                        if (latestVersionPackage != null)
                        {
                            _log.Debug("Package " + latestVersionPackage.Id + " v" + latestVersionPackage.Version +
                                       " is the latest release version.");

                            filtered.Add(latestVersionPackage);
                        }
                        else
                        {
                            _log.Debug("No release version could be found.");
                        }

                        if (latestAbsoluteVersionPackage != null)
                        {
                            _log.Debug("Package " + latestAbsoluteVersionPackage.Id + " v" +
                                       latestAbsoluteVersionPackage.Version + " is the latest prerelease version.");

                            filtered.Add(latestAbsoluteVersionPackage);
                        }
                        else
                        {
                            _log.Debug("No prerelease version could be found.");
                        }
                    }

                    break;
                case FeedImportOptions.VersionSelectorEnum.LatestReleaseVersion:

                    foreach (var packagesGroupedKvp in packagesGroupedById.Value)
                    {
                        var latestReleaseVersionPackage = getLatestReleasePackage(packagesGroupedKvp.Value);

                        if (latestReleaseVersionPackage != null)
                        {
                            _log.Debug("Package " + latestReleaseVersionPackage.Id + " v" +
                                       latestReleaseVersionPackage.Version + " is the latest release version.");

                            filtered.Add(latestReleaseVersionPackage);
                        }
                        else
                        {
                            _log.Debug("No release version could be found.");
                        }
                    }

                    break;
            }

            return filtered;
        }

        private List<IPackage> GetPackages(IPackageRepository remoteRepository, FeedImportOptions options)
        {
            _log.Info("Getting a list of packages to import from " + options.FeedUrl);

            List<IPackage> packages;

            if (options.HasSpecificPackageId)
            {
                if (options.HasSpecificVersion)
                {
                    _log.Debug("Find specific package " + options.SpecificPackageId + " v" + options.Version + " from "+ options.FeedUrl);

                    packages = new List<IPackage>()
                    {
                        remoteRepository.FindPackage(options.SpecificPackageId, new SemanticVersion(options.Version))
                    };
                }
                else if (options.HasVersionSelector)
                {
                    _log.Debug("Find specific package " + options.SpecificPackageId + " using version strategy '" + options.VersionSelector + "' from " + options.FeedUrl);

                    packages = remoteRepository.FindPackagesById(options.SpecificPackageId).ToList();

                    if (!options.IncludePrerelease)
                    {
                        _log.Debug("Excluding prerelease packages");

                        packages = packages.Where(pk => pk.IsReleaseVersion()).ToList();
                    }

                    packages = FilterByVersionSelector(packages, options);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else if (options.HasSearchPackageId)
            {
                _log.Debug("Search packages using " + options.SearchPackageId + " from " + options.FeedUrl);

                if (!options.IncludePrerelease)
                {
                    _log.Debug("Excluding prerelease packages");
                }

                packages = remoteRepository.Search(options.SearchPackageId, options.IncludePrerelease).ToList();

                if (options.HasVersionSelector)
                {
                    _log.Debug("Using version strategy '" + options.VersionSelector + "'");

                    packages = FilterByVersionSelector(packages, options);
                }
            }
            else if (options.IncludeAllPackages)
            {
                _log.Debug("Getting all packages from " + options.FeedUrl);

                var queryablePackages = remoteRepository.GetPackages();

                if (!options.IncludePrerelease)
                {
                    _log.Debug("Excluding prerelease packages");

                    queryablePackages = queryablePackages.Where(pk => pk.IsReleaseVersion());
                }

                packages = queryablePackages.ToList();

                if (options.HasVersionSelector)
                {
                    _log.Debug("Using version strategy '" + options.VersionSelector + "'");

                    packages = FilterByVersionSelector(packages, options);
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            return packages;
        }

        [Serializable]
        public class FeedImportOptions
        {
            public string FeedUrl { get; set; }

            public string SpecificPackageId { get; set; }
            public bool HasSpecificPackageId => !string.IsNullOrWhiteSpace(SpecificPackageId);

            public string SearchPackageId { get; set; }
            public bool HasSearchPackageId => !string.IsNullOrWhiteSpace(SearchPackageId);

            public bool IncludePrerelease { get; set; }

            public bool IncludeAllPackages => !HasSpecificPackageId && !HasSearchPackageId;

            public VersionSelectorEnum? VersionSelector { get; set; }
            public string Version { get; set; }

            public bool HasSpecificVersion => !string.IsNullOrWhiteSpace(Version);
            public bool HasVersionSelector => VersionSelector.HasValue;

            public bool IsValid(out string message)
            {
                if (FeedUrl == null)
                {
                    message = "A feed URL must be specified.";
                    return false;
                }

                if (!HasSpecificVersion && !HasVersionSelector)
                {
                    message = "No versioning strategy was provided.";
                    return false;
                }

                message = "";
                return true;
            }

            public FeedImportOptions WithVersion(string version)
            {
                Version = version;
                return this;
            }

            public FeedImportOptions WithVersion(VersionSelectorEnum selector)
            {
                VersionSelector = selector;
                return this;
            }

            [Serializable]
            public enum VersionSelectorEnum
            {
                AllVersions = 1,
                LatestReleaseVersion = 2,
                LatestReleaseAndPrereleaseVersion = 3,
                SpecificVersion = 4
            }

            public FeedImportOptions IncludePrereleasePackages()
            {
                IncludePrerelease = true;
                return this;
            }

            public FeedImportOptions WithSearchPackageId(string searchTerm)
            {
                SearchPackageId = searchTerm;
                return this;
            }

            public FeedImportOptions WithSpecificPackageId(string packageId)
            {
                SpecificPackageId = packageId;
                return this;
            }
        }

        public class FeedImportStatus
        {
            public FeedImportStatus(int feedId, int totalCount)
            {
                FeedId = feedId;
                RemainingCount = totalCount;
                CompletedCount = 0;
                FailedCount = 0;
                TotalCount = totalCount;
                SuccessfulImports = new List<PackageItem>();
                FailedImports = new List<FailedPackageItem>();
            }

            public List<PackageItem> SuccessfulImports { get; set; }
            public List<FailedPackageItem> FailedImports { get; set; }

            public int FeedId { get; set; }

            public bool IsCompleted { get; set; }
            public int RemainingCount { get; set; }
            public int CompletedCount { get; set; }
            public int FailedCount { get; set; }
            public int TotalCount { get; set; }

            public class PackageItem
            {
                public string PackageId { get; set; }
                public string Version { get; set; }
            }

            public class FailedPackageItem : PackageItem
            {
                public string Error { get; set; }
            }
        }
    }
}