using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
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
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;
        private readonly ILog _log = LogProvider.For<ImportPackagesForFeedJob>();

        public ImportPackagesForFeedJob(IInternalPackageRepositoryFactory factory, IStore store, IFeedService feedService, IPackageService packageService) : base(store)
        {
            _factory = factory;
            _feedService = feedService;
            _packageService = packageService;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Execute(IJobCancellationToken cancellationToken, int feedId, FeedImportOptions options)
        {
            _log.Info("Running import packages job for feed id " + feedId);

            string jobId = JobContext.JobId;

            if (_feedService.GetCount() <= 1)
            {
                _log.Debug("Disabled local cache for package import for feed id " + feedId);

                options.CheckLocalCache = false;
            }

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

            ProcessPackages(feedId, options.FeedUrl, packages, jobId, options.CheckLocalCache);
        }

        private void ProcessPackages(int feedId, string feedUrl, List<IPackage> packages, string jobId, bool useLocalPackages)
        {
            _log.Debug("Enqueuing packages for import for feed id " + feedId);

            Parallel.ForEach(packages, package =>
            {
                BackgroundJob.Enqueue(() => ImportPackage(jobId, feedId, feedUrl, package.Id, package.Version.ToString(), useLocalPackages));
            });

            PackageImportProgressTracker.Instance.WaitUntilComplete(JobContext.JobId);
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

                var existingPackage = localRepository.GetPackage(packageId, version);

                if (existingPackage != null)
                {
                    _log.Warn(
                        "A package with the same ID and version already exists. Overwriting packages is not enabled on this feed. Id = " +
                        packageId + ", Version = " + version);
                    throw new ImportPackageException("A package with the same ID and version already exists.");
                }

                IPackageRepository remoteRepository = PackageRepositoryFactory.Default.CreateRepository(feedUrl);

                DataServicePackage remotePackage =
                    remoteRepository.GetPackages()
                        .Where(pk => pk.Id == packageId)
                        .ToList()
                        .FirstOrDefault(pk => pk.Version == version) as DataServicePackage;

                if (remotePackage == null)
                {
                    _log.Warn("This package was not found on the remote NuGet feed. Id = " + packageId + ", Version = " + version);
                    throw new ImportPackageException("This package was not found on the remote NuGet feed.");
                }

                if (useLocalPackages)
                {
                    if (TryImportFromLocalFeed(parentJobId, feedId, packageId, strVersion, remotePackage,localRepository, version))
                        return;
                }

                localRepository.AddPackage(remotePackage);

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

                _log.ErrorException(message, ex);

                PackageImportProgressTracker.Instance.IncrementFailureCount(parentJobId, new PackageImportProgressAuditItem(packageId, version.ToString(), exception));
                throw new ImportPackageException(ex.Message);
            }
        }

        private bool TryImportFromLocalFeed(string parentJobId, int feedId, string packageId, string strVersion,
            DataServicePackage remotePackage, IInternalPackageRepository localRepository, SemanticVersion version)
        {
            InternalPackage localVersionOfPackage = _packageService.GetPackage(null, packageId, strVersion);

            if (!string.IsNullOrWhiteSpace(localVersionOfPackage?.Hash))
            {
                if (localVersionOfPackage.Hash == remotePackage.PackageHash)
                {
                    IInternalPackageRepository cachedRepo = _factory.Create(localVersionOfPackage.FeedId);

                    var cachePackagePath = cachedRepo.GetPackageFilePath(localVersionOfPackage);

                    if (File.Exists(cachePackagePath))
                    {
                        var cachePackage = FastZipPackage.Open(cachePackagePath, new CryptoHashProvider());

                        cachePackage.Listed = true;

                        localRepository.AddPackage(cachePackage);

                        _log.Info("Completed import of package " + packageId + " v" + strVersion + " to feed id " +
                                  feedId + " using cached package from feed id " + localVersionOfPackage.FeedId);

                        PackageImportProgressTracker.Instance.IncrementSuccessCount(parentJobId,
                            new PackageImportProgressAuditItem(packageId, version.ToString()));
                        return true;
                    }
                }
            }


            return false;
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

                    packages = new List<IPackage>
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

            public bool CheckLocalCache { get; set; }

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