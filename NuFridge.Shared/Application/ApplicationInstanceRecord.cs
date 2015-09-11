using System.Linq;

namespace NuFridge.Shared.Application
{
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