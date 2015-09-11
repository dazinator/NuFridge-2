using System;

namespace NuFridge.Shared.NuGet.Repository
{
    [Serializable]
    public class RemotePackageImportOptions
    {
        public string FeedUrl { get; set; }
        public bool IncludePrerelease { get; set; }
        public bool CheckLocalCache { get; set; }

        public bool IsValid(out string message)
        {
            if (FeedUrl == null)
            {
                message = "A feed URL must be specified.";
                return false;
            }

            message = "";
            return true;
        }

        public RemotePackageImportOptions IncludePrereleasePackages()
        {
            IncludePrerelease = true;
            return this;
        }
    }
}