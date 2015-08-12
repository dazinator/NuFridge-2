using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Logging;
using NuGet;

namespace NuFridge.Shared.Server.NuGet.Import
{
    public class PackageImportRepository : IPackageImportRepository
    {
        private readonly ILog _log = LogProvider.For<PackageImportRepository>();

        private List<IPackage> FilterByVersionSelector(List<IPackage> packages, PackageImportOptions options)
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
                case PackageImportOptions.VersionSelectorEnum.AllVersions:
                    filtered = packages;
                    break;
                case PackageImportOptions.VersionSelectorEnum.LatestReleaseAndPrereleaseVersion:

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
                case PackageImportOptions.VersionSelectorEnum.LatestReleaseVersion:

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

        public List<IPackage> GetPackages(IPackageRepository remoteRepository, PackageImportOptions options)
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

                if (options.HasVersionSelector && options.VersionSelector.Value == PackageImportOptions.VersionSelectorEnum.LatestReleaseAndPrereleaseVersion)
                {
                    options.IncludePrerelease = true;
                }
                else if (options.HasVersionSelector && options.VersionSelector.Value == PackageImportOptions.VersionSelectorEnum.LatestReleaseVersion)
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
                    if (options.VersionSelector.Value == PackageImportOptions.VersionSelectorEnum.AllVersions || options.VersionSelector.Value == PackageImportOptions.VersionSelectorEnum.LatestReleaseAndPrereleaseVersion)
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
    }

    public interface IPackageImportRepository
    {
        List<IPackage> GetPackages(IPackageRepository remoteRepository, PackageImportOptions options);
    }
}