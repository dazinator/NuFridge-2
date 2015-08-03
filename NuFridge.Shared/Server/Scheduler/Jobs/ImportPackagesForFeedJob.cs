using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage.Monitoring;
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
    [Queue("filesystem")]
    public class ImportPackagesForFeedJob : PackagesBase
    {
        private readonly IInternalPackageRepositoryFactory _factory;
        private readonly ILog _log = LogProvider.For<ImportPackagesForFeedJob>();

        public ImportPackagesForFeedJob(IInternalPackageRepositoryFactory factory, IStore store) : base(store)
        {
            _factory = factory;
        }

        [AutomaticRetryAttribute(Attempts = 0)]
        public void Execute(IJobCancellationToken cancellationToken, int feedId, FeedImportOptions options)
        {
            _log.Info("Running import packages job for feed id " + feedId);

            string jobId = JobContext.JobId;

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ImportPackagesHub>();

            List<IPackage> packages;

            try
            {
                var remoteRepository = PackageRepositoryFactory.Default.CreateRepository(options.FeedUrl);
                packages = GetPackages(remoteRepository, options);
            }
            catch (Exception ex)
            {
                _log.ErrorException("There was an error getting the list of packages to import.", ex);
                PackageImportProgressTracker.Instance.ReportStartFailure(hubContext, jobId, "The package import failed to start. " + ex.Message);
                throw;
            }

            _log.Info("Found " + packages.Count + " for import for feed id " + feedId + " from " + options.FeedUrl);



            PackageImportProgressTracker.Instance.AddJob(hubContext, jobId, feedId, packages.Count());

            ProcessPackages(feedId, options.FeedUrl, packages, jobId);
        }

        private void ProcessPackages(int feedId, string feedUrl, List<IPackage> packages, string jobId)
        {
            _log.Debug("Enqueuing packages for import for feed id " + feedId);

            Parallel.ForEach(packages, package =>
            {
                BackgroundJob.Enqueue(() => ImportPackage(jobId, feedId, feedUrl, package.Id, package.Version.ToString()));
            });

            PackageImportProgressTracker.Instance.WaitUntilComplete(JobContext.JobId);
        }

        [AutomaticRetry(Attempts = PackageImportProgressTracker.RetryCount)]
        public void ImportPackage(string parentJobId, int feedId, string feedUrl, string packageId, string strVersion)
        {
            SemanticVersion version = new SemanticVersion(strVersion);

            try
            {
                _log.Debug("Beginning import of package " + packageId + " v" + strVersion + " to feed id " + feedId);

                IInternalPackageRepository localRepository = _factory.Create(feedId);

                var existingPackage = localRepository.GetPackage(packageId, version);

                if (existingPackage != null)
                {
                    _log.Warn(
                        "A package with the same ID and version already exists. Overwriting packages is not enabled on this feed. Id = " +
                        packageId + ", Version = " + version);
                    throw new ImportPackageException("A package with the same ID and version already exists.");
                }

                IPackageRepository remoteRepository = PackageRepositoryFactory.Default.CreateRepository(feedUrl);

                // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault - The method 'FirstOrDefault' is not supported.
                IPackage package =
                    remoteRepository.GetPackages()
                        .Where(pk => pk.Id == packageId)
                        .ToList()
                        .FirstOrDefault(pk => pk.Version == version);

                if (package == null)
                {
                    _log.Warn("This package was not found on the remote NuGet feed. Id = " + packageId + ", Version = " + version);
                    throw new ImportPackageException("This package was not found on the remote NuGet feed.");
                }


                localRepository.AddPackage(package);

                _log.Info("Completed import of package " + packageId + " v" + strVersion + " to feed id " + feedId);

                PackageImportProgressTracker.Instance.IncrementSuccessCount(parentJobId, new PackageImportProgressAuditItem(packageId, version.ToString()));
            }
            catch (Exception ex)
            {
                var message = "There was an error importing the " + packageId + " package v" + strVersion +
                              " to the feed " + feedId + ".";

                string exception = "Exception: " + ex.Message;

                if (ex.InnerException != null)
                {
                    exception += "\r\nInner Exception: " + ex.InnerException.Message;
                }

                _log.ErrorException(message + "\r\n" + exception, ex);

                PackageImportProgressTracker.Instance.IncrementFailureCount(parentJobId, new PackageImportProgressAuditItem(packageId, version.ToString(), exception));
                throw new ImportPackageException(ex.Message);
            }
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

            Func<List<IPackage>, IPackage> getLatestPrereleasePackage = delegate (List<IPackage> groupedPackages)
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
                    _log.Debug("Find specific package " + options.SpecificPackageId + " v" + options.Version + " from " + options.FeedUrl);

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

                if (options.HasVersionSelector && options.VersionSelector.Value == FeedImportOptions.VersionSelectorEnum.LatestReleaseAndPrereleaseVersion)
                {
                    options.IncludePrerelease = true;
                }
                else if (options.HasVersionSelector && options.VersionSelector.Value == FeedImportOptions.VersionSelectorEnum.LatestReleaseVersion)
                {
                    options.IncludePrerelease = false;
                }

                if (!options.IncludePrerelease)
                {
                    _log.Debug("Excluding prerelease packages");
                }

                packages = remoteRepository.Search(options.SearchPackageId, options.IncludePrerelease).ToList();

                if (options.HasVersionSelector)
                {
                    if (options.VersionSelector.Value == FeedImportOptions.VersionSelectorEnum.AllVersions || options.VersionSelector.Value == FeedImportOptions.VersionSelectorEnum.LatestReleaseAndPrereleaseVersion)
                    {
                        List<IPackage> packagesToAdd = new List<IPackage>();
                        object sync = new object();

                        Parallel.ForEach(packages, package =>
                        {
                            var otherVersions = remoteRepository.FindPackagesById(package.Id);
                            var toAdd = otherVersions.Where(ov => packages.All(pk => pk.Version != ov.Version)).ToList();
                            lock (sync)
                            {
                                packagesToAdd.AddRange(toAdd);
                            }
                        });

                        packages.AddRange(packagesToAdd);
                    }

                    if (!options.IncludePrerelease)
                    {
                        _log.Debug("Excluding prerelease packages");

                        packages = packages.Where(pk => pk.IsReleaseVersion()).ToList();
                    }

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

                if (HasVersionSelector)
                {
                    if (VersionSelector.Value == VersionSelectorEnum.SpecificVersion)
                    {
                        if (string.IsNullOrWhiteSpace(Version))
                        {
                            message = "No specific version provided.";
                            return false;
                        }
                    }
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

        public class ImportPackageException : Exception
        {
            public ImportPackageException(string message) : base(message)
            {

            }
        }
    }
}