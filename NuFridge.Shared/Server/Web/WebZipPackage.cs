using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model;
using NuGet;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet.FastZipPackage;

namespace NuFridge.Shared.Server.Web
{
    public class WebZipPackage : FastZipPackageBase, IWebPackage
    {
        public Uri Uri { get; private set; }

        private IPackage Package { get; set; }

        public WebZipPackage(IPackage package,  Uri uri)
            : base()
        {
            Uri = uri;
            Package = package;
        }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<IPackageFile> GetFiles()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<System.Runtime.Versioning.FrameworkName> GetSupportedFrameworks()
        {
            throw new NotImplementedException();
        }

        public bool IsAbsoluteLatestVersion
        {
            get { return Package.IsAbsoluteLatestVersion; }
        }

        public bool IsLatestVersion
        {
            get { return Package.IsLatestVersion; }
        }

        public bool Listed
        {
            get { return Package.Listed; }
        }

        public DateTimeOffset? Published
        {
            get { return Package.Published; }
        }

        public IEnumerable<string> Authors
        {
            get { return Package.Authors; }
        }

        public string Copyright
        {
            get { return Package.Copyright; }
        }

        public IEnumerable<PackageDependencySet> DependencySets
        {
            get { return new List<PackageDependencySet>(); }
        }

        public string Description
        {
            get { return Package.Description; }
        }

        public bool DevelopmentDependency
        {
            get { return Package.DevelopmentDependency; }
        }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies
        {
            get { return new List<FrameworkAssemblyReference>(); }
        }

        public Uri IconUrl
        {
            get { return Package.IconUrl; }
        }

        public string Language
        {
            get { return Package.Language; }
        }

        public Uri LicenseUrl
        {
            get { return Package.LicenseUrl; }
        }

        public Version MinClientVersion
        {
            get { return new Version(); }
        }

        public IEnumerable<string> Owners
        {
            get { return Package.Owners; }
        }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences
        {
            get { return new List<PackageReferenceSet>();}
        }

        public Uri ProjectUrl
        {
            get { return Package.ProjectUrl; }
        }

        public string ReleaseNotes
        {
            get { return Package.ReleaseNotes; }
        }

        public bool RequireLicenseAcceptance
        {
            get { return Package.RequireLicenseAcceptance; }
        }

        public string Summary
        {
            get { return Package.Summary; }
        }

        public string Tags
        {
            get { return Package.Tags; }
        }

        public string Title
        {
            get { return Package.Title; }
        }

        public string Id
        {
            get { return Package.Id; }
        }

        public SemanticVersion Version
        {
            get { return Package.Version; }
        }

        public int DownloadCount
        {
            get { return Package.DownloadCount; }
        }

        public Uri ReportAbuseUrl
        {
            get { return null; }
        }

        public override Stream GetStream()
        {
            return Stream.Null;
        }
    }
}
