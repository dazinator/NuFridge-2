using System;
using System.Configuration;
using System.IO;

namespace NuFridge.Service.Feeds
{
    public sealed class ServiceConfiguration
    {
        private const string FeedsHomeKey = "NuFridge.FeedsPath";
        private const string DebuggingToolsPathKey = "NuFridge.DebuggingToolsPath";
        private const string WebsiteBindingKey = "NuFridge.AdministrationWebsite.Binding";

        private string _feedsHome;
        public string FeedsHomePath
        {
            get
            {
                return _feedsHome ?? (_feedsHome = ConfigurationManager.AppSettings[FeedsHomeKey]);
            }
        }

        private string _websiteBinding;
        public string WebsiteBinding
        {
            get
            {
                return _websiteBinding ?? (_websiteBinding = ConfigurationManager.AppSettings[WebsiteBindingKey]);
            }
        }

        private string _debuggingToolsPath;
        public string DebuggingToolsPath
        {
            get
            {
                return _debuggingToolsPath ?? (_debuggingToolsPath = ConfigurationManager.AppSettings[DebuggingToolsPathKey]);
            }
        }

        public ConfigurationValidateResult Validate()
        {
            if (FeedsHomePath == null)
            {
                return new ConfigurationValidateResult(new NullReferenceException(string.Format("The '{0}' app setting has not been set.", FeedsHomePath)));
            }

            if (!System.IO.Directory.Exists(FeedsHomePath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(FeedsHomePath);
                }
                catch (Exception ex)
                {
                    return new ConfigurationValidateResult(ex);
                }
            }

            var currentAssemblyPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var currentDirectory = Directory.GetParent(currentAssemblyPath);

            var appPath = Path.Combine(currentDirectory.FullName, "app");

            if (!Directory.Exists(appPath))
            {
              //  return new ConfigurationValidateResult(new DirectoryNotFoundException(string.Format("Could not find the application at '{0}'.", appPath)));
            }

            return new ConfigurationValidateResult();
        }

        public sealed class ConfigurationValidateResult
        {
            public bool Success { get; private set; }
            public Exception Exception { get; private set; }

            public ConfigurationValidateResult()
            {
                Success = true;
            }

            public ConfigurationValidateResult(Exception ex)
            {
                Success = false;
                Exception = ex;
            }
        }
    }
}