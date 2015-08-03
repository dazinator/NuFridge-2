using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using NuFridge.Shared.Server.Application;

namespace NuFridge.Shared.Server.Configuration
{
    public class HomeConfiguration : IHomeConfiguration
    {
        private readonly IApplicationInstanceSelector _instance;

        public string InstallDirectory { get; set; }
        public string ConnectionString { get; set; }
        public string ListenPrefixes { get; set; }
        public string WindowsDebuggingToolsPath { get; set; }


        public string NuGetFrameworkNames
        {
            get { return _instance.Current.NuGetFrameworkNames; }
            set { _instance.Current.NuGetFrameworkNames = value; }
        }

        private bool TryParseEnum<T>(string str, out T value) where T : struct
        {
            var names = Enum.GetNames(typeof(T));
            value = (Enum.GetValues(typeof(T)) as T[])[0];
            foreach (var name in names)
            {
                if (String.Equals(name, str, StringComparison.OrdinalIgnoreCase))
                {
                    value = (T)Enum.Parse(typeof(T), name);
                    return true;
                }
            }
            return false;
        }

        public HomeConfiguration(IApplicationInstanceSelector instance)
        {
            _instance = instance;

            if (instance.Current == null)
            {
                instance.LoadInstance();
            }

            InstallDirectory = instance.Current.InstallDirectory;
            ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            ListenPrefixes = ConfigurationManager.AppSettings["WebsiteUrl"];
            WindowsDebuggingToolsPath = ConfigurationManager.AppSettings["WindowsDebuggingToolsPath"];
        }

        public void Save()
        {
            _instance.Save();
        }
    }
}