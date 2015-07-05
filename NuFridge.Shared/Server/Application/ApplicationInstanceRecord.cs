using System.Linq;

namespace NuFridge.Shared.Server.Application
{
    //This whole class is bad. TODO
    public class ApplicationInstanceRecord
    {
        public string InstallDirectory { get; set; }


        public const string InstallDirectoryKey = "InstallationDirectory";

        public bool IsValid()
        {
            var items = new[] { InstallDirectory };

            return items.All(item => !string.IsNullOrWhiteSpace(item));
        }
    }
}