using System.Linq;

namespace NuFridge.Shared.Server.Application
{
    //This whole class is bad. TODO
    public class ApplicationInstanceRecord
    {
        public string InstallDirectory { get; set; }
        public string SqlDataSource { get; set; }
        public string SqlInitialCatalog { get; set; }
        public string SqlUsername { get; set; }
        public string SqlPassword { get; set; }
        public string ListenPrefixes { get; set; }

        public const string InstallDirectoryKey = "InstallationDirectory";
        public const string SqlDataSourceKey = "SqlDataSource";
        public const string SqlInitialCatalogKey = "SqlInitialCatalog";
        public const string SqlUsernameKey = "SqlUsername";
        public const string SqlPasswordKey = "SqlPassword";
        public const string ListenPrefixesKey = "ListenPrefixes";

        public bool IsValid()
        {
            var items = new[] { InstallDirectory, SqlDataSource, SqlInitialCatalog, SqlUsername, SqlPassword, ListenPrefixes };

            return items.All(item => !string.IsNullOrWhiteSpace(item));
        }
    }
}