using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NuGet.Lucene;
using NuGet.Lucene.Web;

namespace NuFridge.Service.Feeds
{
    namespace NuGet.Lucene.Web
    {
        public class NuGetFeedSettings : INuGetWebApiSettings
        {
            public const string BlankAppSettingPrefix = "";

            public const string DefaultRoutePathPrefix = "api/";



            private readonly System.Collections.Specialized.NameValueCollection roleMappings;
            private string FeedDirectory { get; set; }
            private ServiceConfiguration Config { get; set; }

            public NuGetFeedSettings(ServiceConfiguration config, string feedDirectory)
            {
                this.Config = config;
                this.roleMappings = roleMappings ?? new NameValueCollection();

                FeedDirectory = feedDirectory;
            }

            public bool ShowExceptionDetails
            {
                get { return true; }
            }

            public bool EnableCrossDomainRequests
            {
                get { return true; }
            }

            public bool HandleLocalRequestsAsAdmin
            {
                get { return true; }
            }

            public string LocalAdministratorApiKey
            {
                get { return Config.ApiKey; }
            }

            public bool AllowAnonymousPackageChanges
            {
                get { return false; }
            }

            public string RoutePathPrefix
            {
                get { return DefaultRoutePathPrefix; }
            }

            public string PackageMirrorTargetUrl
            {
                get { return ""; }
            }

            public bool AlwaysCheckMirror
            {
                get { return false; }
            }

            public TimeSpan PackageMirrorTimeout
            {
                get
                {
                    var str = "0:00:15";
                    TimeSpan ts;
                    return TimeSpan.TryParse(str, out ts) ? ts : TimeSpan.FromSeconds(15);
                }
            }

            public bool RoleMappingsEnabled
            {
                get
                {
                    var mappings = RoleMappings;
                    return mappings.AllKeys.Any(key => !String.IsNullOrWhiteSpace(mappings.Get((string) key)));
                }
            }

            public System.Collections.Specialized.NameValueCollection RoleMappings
            {
                get
                {
                    return roleMappings;
                }
            }

            public string SymbolsPath
            {
                get
                {
                    return Path.Combine(FeedDirectory, @"Symbols");
                }
            }

            public string ToolsPath
            {
                get { return Config.DebuggingToolsPath; }
            }

            public bool KeepSourcesCompressed
            {
                get { return true; }
            }

            public bool SynchronizeOnStart
            {
                get { return Config.SynchronizeOnStart; }
            }

            public bool EnablePackageFileWatcher
            {
                get { return Config.EnablePackageFileWatcher; }
            }

            public bool GroupPackageFilesById
            {
                get { return Config.GroupPackageFilesById; }
            }

            public string LucenePackagesIndexPath
            {
                get
                {
                    return Path.Combine(FeedDirectory, @"Lucene");
                }
            }

            public string PackagesPath
            {
                get
                {
                    return Path.Combine(FeedDirectory, @"Packages");
                }
            }

            public PackageOverwriteMode PackageOverwriteMode
            {
                get
                {
                    if (Config.AllowPackageOverwrite)
                    {
                        return PackageOverwriteMode.Allow;
                    }

                    return PackageOverwriteMode.Deny;
                }
            }

            public string LuceneUsersIndexPath
            {
                get
                {
                    return Path.Combine(LucenePackagesIndexPath, "Users");
                }
            }

            public int LuceneMergeFactor
            {
                get
                {
                    return 0;
                }
            }
        }
    }

}
