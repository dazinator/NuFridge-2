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

        public PackageImportOptions IncludePrereleasePackages()
        {
            IncludePrerelease = true;
            return this;
        }
    }
}