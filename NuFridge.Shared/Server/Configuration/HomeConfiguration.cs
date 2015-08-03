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
    }
}