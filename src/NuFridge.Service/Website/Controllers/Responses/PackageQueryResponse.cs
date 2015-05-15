using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Service.Website.Controllers.Responses
{
    public class PackageQueryResponse
    {
        public string query { get; set; }
        public bool includePrerelease { get; set; }
        public int totalHits { get; set; }
        public string originFilter { get; set; }
        public string sort { get; set; }
        public string order { get; set; }
        public int offset { get; set; }
        public int count { get; set; }
        public string elapsedPreparationTime { get; set; }
        public string elapsedSearchTime { get; set; }
        public string elapsedRetrievalTime { get; set; }
        public string computedQuery { get; set; }
        public List<Hit> hits { get; set; }

        public class Hit
        {
            public double score { get; set; }
            public string id { get; set; }
            public string version { get; set; }
            public object minClientVersion { get; set; }
            public string title { get; set; }
            public string iconUrl { get; set; }
            public string licenseUrl { get; set; }
            public string projectUrl { get; set; }
            public object reportAbuseUrl { get; set; }
            public bool requireLicenseAcceptance { get; set; }
            public string searchId { get; set; }
            public string displayTitle { get; set; }
            public string description { get; set; }
            public string summary { get; set; }
            public string releaseNotes { get; set; }
            public string language { get; set; }
            public string tags { get; set; }
            public object copyright { get; set; }
            public int downloadCount { get; set; }
            public int versionDownloadCount { get; set; }
            public bool isAbsoluteLatestVersion { get; set; }
            public bool isLatestVersion { get; set; }
            public bool developmentDependency { get; set; }
            public bool listed { get; set; }
            public bool isPrerelease { get; set; }
            public string published { get; set; }
            public List<string> authors { get; set; }
            public List<string> owners { get; set; }
            public List<object> dependencies { get; set; }
            public List<object> dependencySets { get; set; }
            public List<object> frameworkAssemblies { get; set; }
            public List<object> assemblyReferences { get; set; }
            public object packageAssemblyReferences { get; set; }
            public int packageSize { get; set; }
            public string packageHash { get; set; }
            public string packageHashAlgorithm { get; set; }
            public string lastUpdated { get; set; }
            public string created { get; set; }
            public string path { get; set; }
            public List<object> supportedFrameworks { get; set; }
            public List<string> files { get; set; }
            public object originUrl { get; set; }
            public bool isMirrored { get; set; }
        }

    }
}