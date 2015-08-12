using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Server.NuGet.Import
{
    [Serializable]
    public class PackageImportOptions
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

        public PackageImportOptions WithVersion(string version)
        {
            Version = version;
            return this;
        }

        public PackageImportOptions WithVersion(VersionSelectorEnum selector)
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

        public PackageImportOptions IncludePrereleasePackages()
        {
            IncludePrerelease = true;
            return this;
        }

        public PackageImportOptions WithSearchPackageId(string searchTerm)
        {
            SearchPackageId = searchTerm;
            return this;
        }

        public PackageImportOptions WithSpecificPackageId(string packageId)
        {
            SpecificPackageId = packageId;
            return this;
        }
    }
}