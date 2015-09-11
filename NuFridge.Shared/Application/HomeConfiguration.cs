using System.Configuration;
using System.IO;
using System.Reflection;

namespace NuFridge.Shared.Application
{
    public class HomeConfiguration : IHomeConfiguration
    {
        public string InstallDirectory { get; set; }
        public string WebsiteDirectory { get; set; }
        public string ConnectionString { get; set; }
        public string ListenPrefixes { get; set; }
        public bool DatabaseReadOnly { get; set; }
        public string WindowsDebuggingToolsPath { get; set; }

        public HomeConfiguration(IApplicationInstanceSelector instance)
        {
            if (instance.Current == null)
            {
                instance.LoadInstance();
            }

            InstallDirectory = instance.Current.InstallDirectory;
            ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            ListenPrefixes = ConfigurationManager.AppSettings["WebsiteUrl"];
            WindowsDebuggingToolsPath = ConfigurationManager.AppSettings["WindowsDebuggingToolsPath"];
            DatabaseReadOnly = bool.Parse(ConfigurationManager.AppSettings["DatabaseReadOnly"]);

            var entryLocation = Assembly.GetEntryAssembly().Location;
            var currentFolder = Directory.GetParent(entryLocation).FullName;

            WebsiteDirectory = Path.Combine(currentFolder, "Website");

#if DEBUG
            var entryAssemblyFile = Assembly.GetEntryAssembly().Location;
            var entryAssemblyFolder = Directory.GetParent(entryAssemblyFile).FullName;
            var binFolder = Directory.GetParent(entryAssemblyFolder).FullName;
            var serviceFolder = Directory.GetParent(binFolder).FullName;
            var solutionFolder = Directory.GetParent(serviceFolder).FullName;

            WebsiteDirectory = Path.Combine(solutionFolder, "NuFridge.Website.New");
#endif

            if (!Directory.Exists(WebsiteDirectory))
            {
                throw new DirectoryNotFoundException("Failed to find the website folder at " + WebsiteDirectory);
            }
        }
    }
}