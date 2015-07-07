using System.Linq;

namespace NuFridge.Shared.Server.Application
{
    public class ApplicationInstanceRecord
    {
        public string InstallDirectory { get; set; }
        public string NuGetFrameworkNames { get; set; }


        public const string InstallDirectoryKey = "InstallationDirectory";
        public const string NuGetFrameworkNamesKey = "NuGetFrameworkNames";

        public bool IsValid()
        {
            var items = new[] { InstallDirectory };

            return items.All(item => !string.IsNullOrWhiteSpace(item));
        }
    }
}