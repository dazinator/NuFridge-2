using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NuFridge.Service.Model;
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


            private string FeedDirectory { get; set; }
            private string ApiKey { get; set; }
            private ServiceConfiguration Config { get; set; }
            private Feed Feed { get; set; }

            public NuGetFeedSettings(Feed feed)
            {
                this.Config = new ServiceConfiguration();
                this.Feed = feed;

                FeedDirectory = Path.GetFullPath(Path.Combine(Config.FeedsHomePath, feed.Id));

                if (!Directory.Exists(FeedDirectory))
                {
                    Directory.CreateDirectory(FeedDirectory);
                }

                if (!Directory.Exists(PackagesPath))
                {
                    Directory.CreateDirectory(PackagesPath);
                }

                if (!Directory.Exists(LucenePackagesIndexPath))
                {
                    Directory.CreateDirectory(LucenePackagesIndexPath);
                }

                if (!Directory.Exists(LuceneUsersIndexPath))
                {
                    Directory.CreateDirectory(LuceneUsersIndexPath);
                }

                if (!Directory.Exists(SymbolsPath))
                {
                    Directory.CreateDirectory(SymbolsPath);
                }
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
                get { return ApiKey; }
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

            public NameValueCollection RoleMappings
            {
                get
                {
                    return new NameValueCollection();
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
                get { return Feed.SynchronizeOnStart; }
            }

            public bool EnablePackageFileWatcher
            {
                get { return Feed.EnablePackageFileWatcher; }
            }

            public bool GroupPackageFilesById
            {
                get { return Feed.GroupPackageFilesById; }
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
                    if (Feed.AllowPackageOverwrite)
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


            public bool DisablePackageHash
            {
                get { return false; }
            }

            public bool IgnorePackageFiles
            {
                get { return false; }
            }
        }
    }

}
