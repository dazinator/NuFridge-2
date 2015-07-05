using System.Configuration;
using NuFridge.Shared.Server.Application;

namespace NuFridge.Shared.Server.Configuration
{
    public class HomeConfiguration : IHomeConfiguration
    {
        public string InstallDirectory { get; set; }
        public string SqlDataSource { get; set; }
        public string SqlInitialCatalog { get; set; }
        public string SqlUsername { get; set; }
        public string SqlPassword { get; set; }
        public string ListenPrefixes { get; set; }

        public HomeConfiguration(IApplicationInstanceSelector instance)
        {
            InstallDirectory = instance.Current.InstallDirectory;
            SqlDataSource = ConfigurationManager.AppSettings["SqlServer"];
            SqlInitialCatalog = ConfigurationManager.AppSettings["SqlDatabase"];
            SqlUsername = ConfigurationManager.AppSettings["SqlUserId"];
            SqlPassword = ConfigurationManager.AppSettings["SqlPassword"];
            ListenPrefixes = ConfigurationManager.AppSettings["WebsiteUrl"];
        }
    }
}