using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NuGet.Lucene;
using NuGet.Lucene.Web;

namespace NuFridge.Common
{
    namespace NuGet.Lucene.Web
    {
        public class NuGetFeedSettings : INuGetWebApiSettings
        {
            public const string DefaultAppSettingPrefix = "NuGet.Lucene.Web:";
            public const string BlankAppSettingPrefix = "";

            public const string DefaultRoutePathPrefix = "api/";

            private readonly string prefix;
            private readonly System.Collections.Specialized.NameValueCollection settings;
            private readonly System.Collections.Specialized.NameValueCollection roleMappings;
            private string FeedDirectory { get; set; }

            public NuGetFeedSettings(string feedDirectory)
                : this(DefaultAppSettingPrefix, feedDirectory)
            {
            }



            public NuGetFeedSettings(string prefix, string feedDirectory)
            {
                this.prefix = prefix;
                this.settings = settings;
                this.roleMappings = roleMappings ?? new NameValueCollection();

                if (!Directory.Exists((feedDirectory)))
                {
                    throw new DirectoryNotFoundException("Could not find a directory at " + feedDirectory);
                }

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
                get { return "Temp"; }
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
                    return Path.Combine(FeedDirectory, @"App_Data\Symbols");
                }
            }

            public string ToolsPath
            {
                get { return ""; }
            }

            public bool KeepSourcesCompressed
            {
                get { return true; }
            }

            public bool SynchronizeOnStart
            {
                get { return true; }
            }

            public bool EnablePackageFileWatcher
            {
                get { return true; }
            }

            public bool GroupPackageFilesById
            {
                get { return true; }
            }

            public string LucenePackagesIndexPath
            {
                get
                {
                    return Path.Combine(FeedDirectory, @"App_Data\Lucene");
                }
            }

            public string PackagesPath
            {
                get
                {
                    return Path.Combine(FeedDirectory, @"App_Data\Packages");
                }
            }

            public PackageOverwriteMode PackageOverwriteMode
            {
                get { return global::NuGet.Lucene.PackageOverwriteMode.Allow; }
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
