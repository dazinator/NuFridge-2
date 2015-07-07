using System.Configuration;
using System.Linq;
using NuFridge.Shared.Server.Application;

namespace NuFridge.Shared.Server.Configuration
{
    public class HomeConfiguration : IHomeConfiguration
    {
        private readonly IApplicationInstanceSelector _instance;

        public string InstallDirectory { get; set; }
        public string SqlDataSource { get; set; }
        public string SqlInitialCatalog { get; set; }
        public string SqlUsername { get; set; }
        public string SqlPassword { get; set; }
        public string ListenPrefixes { get; set; }

        public string NuGetFrameworkNames
        {
            get { return _instance.Current.NuGetFrameworkNames; }
            set { _instance.Current.NuGetFrameworkNames = value; }
        }

        public HomeConfiguration(IApplicationInstanceSelector instance)
        {
            _instance = instance;

            if (instance.Current == null)
            {
                instance.LoadInstance();
            }

            InstallDirectory = instance.Current.InstallDirectory;
            SqlDataSource = ConfigurationManager.AppSettings["SqlServer"];
            SqlInitialCatalog = ConfigurationManager.AppSettings["SqlDatabase"];
            SqlUsername = ConfigurationManager.AppSettings["SqlUserId"];
            SqlPassword = ConfigurationManager.AppSettings["SqlPassword"];
            ListenPrefixes = ConfigurationManager.AppSettings["WebsiteUrl"];
        }

        public void Save()
        {
            _instance.Save();
        }
    }
}