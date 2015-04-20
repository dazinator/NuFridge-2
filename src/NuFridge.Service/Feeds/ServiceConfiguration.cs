using System;
using System.Configuration;
using System.IO;

namespace NuFridge.Service.Feeds
{
    public sealed class ServiceConfiguration
    {
        private const string FeedWebBindingKey = "NuFridge.Feeds.Binding";
        private const string FeedsHomeKey = "NuFridge.Feeds.Home";
        private const string DebuggingToolsPathKey = "NuFridge.DebuggingToolsPath";
        private const string SynchronizeOnStartKey = "NuFridge.Feeds.SynchronizeOnStart";
        private const string EnablePackageFileWatcherKey = "NuFridge.Feeds.EnablePackageFileWatcher";
        private const string GroupPackageFilesByIdKey = "NuFridge.Feeds.GroupPackageFilesById";
        private const string ApiKeyKey = "NuFridge.Feeds.ApiKey";
        private const string AllowPackageOverwriteKey = "NuFridge.Feeds.AllowPackageOverwrite";
        private const string WebsiteBindingKey = "NuFridge.AdministrationWebsite.Binding";

        private bool? _allowPackageOverwrite;
        public bool AllowPackageOverwrite
        {
            get
            {
                if (!_allowPackageOverwrite.HasValue)
                {
                    bool tmpValue;
                    if (bool.TryParse(ConfigurationManager.AppSettings[AllowPackageOverwriteKey], out tmpValue))
                    {
                        _allowPackageOverwrite = tmpValue;
                    }
                }

                return _allowPackageOverwrite ?? (_allowPackageOverwrite = true).Value;
            }
        }

        private string _apiKey;
        public string ApiKey
        {
            get
            {
                return _apiKey ?? (_apiKey = ConfigurationManager.AppSettings[ApiKeyKey]);
            }
        }

        private bool? _groupPackageFilesById;
        public bool GroupPackageFilesById
        {
            get
            {
                if (!_groupPackageFilesById.HasValue)
                {
                    bool tmpValue;
                    if (bool.TryParse(ConfigurationManager.AppSettings[GroupPackageFilesByIdKey], out tmpValue))
                    {
                        _groupPackageFilesById = tmpValue;
                    }
                }

                return _groupPackageFilesById ?? (_groupPackageFilesById = true).Value;
            }
        }

        private bool? _enablePackageFileWatcher;
        public bool EnablePackageFileWatcher
        {
            get
            {
                if (!_enablePackageFileWatcher.HasValue)
                {
                    bool tmpValue;
                    if (bool.TryParse(ConfigurationManager.AppSettings[EnablePackageFileWatcherKey], out tmpValue))
                    {
                        _enablePackageFileWatcher = tmpValue;
                    }
                }

                return _enablePackageFileWatcher ?? (_enablePackageFileWatcher = true).Value;
            }
        }

        private bool? _synchronizeOnStart;
        public bool SynchronizeOnStart
        {
            get
            {
                if(!_synchronizeOnStart.HasValue)
                {
                    bool tmpValue;
                    if (bool.TryParse(ConfigurationManager.AppSettings[SynchronizeOnStartKey], out tmpValue))
                    {
                        _synchronizeOnStart = tmpValue;
                    }
                }
                
                return _synchronizeOnStart ?? (_synchronizeOnStart = true).Value;
            }
        }

        private string _feedsHome;
        public string FeedsHome
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

        private string _feedWebBinding;
        public string FeedWebBinding
        {
            get
            {
                return _feedWebBinding ?? (_feedWebBinding = ConfigurationManager.AppSettings[FeedWebBindingKey]);
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
            if (FeedWebBinding == null)
            {
                return new ConfigurationValidateResult(new NullReferenceException(string.Format("The '{0}' app setting has not been set.", FeedWebBindingKey)));
            }

            if (FeedsHome == null)
            {
                return new ConfigurationValidateResult(new NullReferenceException(string.Format("The '{0}' app setting has not been set.", FeedsHome)));
            }

            if (!System.IO.Directory.Exists(FeedsHome))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(FeedsHome);
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