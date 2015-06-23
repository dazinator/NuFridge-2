using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml.Serialization;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Security;
using NuGet;

namespace NuFridge.Shared.Model
{
    public class InternalPackage : IInternalPackage, IEntity, IPackage
    {
        public InternalPackage()
        {
            IsAbsoluteLatestVersion = true;
        }

        public int Id { get; set; }

        public int FeedId { get; set; }

        public string PackageId { get; set; }

        public string FullVersion { get { return Version == null ? string.Empty : Version.ToString(); } }

        [XmlIgnore]
        private SemanticVersion _version { get; set; }

        public SemanticVersion Version
        {
            get
            {
                return _version; ;
            }
            set
            {
                _version = value;
                if (value != null)
                {
                    VersionBuild = value.Version.Build;
                    VersionMinor = value.Version.Minor;
                    VersionMajor = value.Version.Major;
                    VersionRevision = value.Version.Revision;
                    VersionSpecial = value.SpecialVersion;
                }
                else
                {
                    VersionBuild = 0;
                    VersionMinor = 0;
                    VersionMajor = 0;
                    VersionRevision = 0;
                    VersionSpecial = null;
                }
            }
        }

        public int VersionMajor { get; set; }
        public int VersionMinor { get; set; }
        public int VersionBuild { get; set; }
        public int VersionRevision { get; set; }
        public string VersionSpecial { get; set; }

        public string Description { get; set; }

        public string ReleaseNotes { get; set; }

        public string Copyright { get; set; }

        public DateTimeOffset? Published { get; set; }

        public string[] Dependencies { get; set; }

        public string PackageHash { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public long Size { get; set; }

        public bool IsReleaseVersion()
        {
            return string.IsNullOrWhiteSpace(Version.SpecialVersion);
        }

        public long GetSize()
        {
            return Size;
        }

        public List<string> GetDependencies()
        {
            return Dependencies.ToList();
        }

        public string CalculateHash()
        {
            return PackageHash;
        }

        public static InternalPackage Create(int feedId, IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion)
        {
            var newPackage = new InternalPackage
             {
                 PackageId = package.Id,
                 FeedId = feedId,
                 Version = package.Version,
                 Description = package.Description,
                 Published = package.Published ?? DateTimeOffset.UtcNow,
                 LastUpdated = DateTime.UtcNow,
                 ReleaseNotes = package.ReleaseNotes,
                 Summary = package.Summary,
                 Title = package.Title,
                 DownloadCount = package.DownloadCount,
                 IsAbsoluteLatestVersion = isAbsoluteLatestVersion,
                 IsLatestVersion = isLatestVersion,
                 Copyright = package.Copyright,
                 IsPrerelease = !package.IsReleaseVersion(),
                 LicenseUrl = package.LicenseUrl,
                 Listed = true,
                 ProjectUrl = package.ProjectUrl,
                 RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                 Tags = package.Tags,
                 IconUrl = package.IconUrl
             };

            if (package.Owners != null)
            {
                newPackage.Owners = package.Owners.ToArray();
            }

            if (package.Authors != null)
            {
                newPackage.Authors = package.Authors.ToArray();
            }

            using (Stream stream = package.GetStream())
            {
                newPackage.PackageHash = HashCalculator.Hash(stream);
                newPackage.Size = stream.Length;
            }

            return newPackage;
        }


        public string Name
        {
            get { return PackageId; }
        }

        public string DisplayTitle { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Language { get; set; }
        public string Tags { get; set; }
        public string PackageHashAlgorithm { get; set; }
        public long PackageSize { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime Created { get; set; }
        public bool IsAbsoluteLatestVersion { get; set; }
        public bool IsLatestVersion { get; set; }
        public bool IsPrerelease { get; set; }
        public bool Listed { get; set; }
        public int DownloadCount { get; set; }
        public int VersionDownloadCount { get; set; }
        public bool DevelopmentDependency { get; set; }
        public float Score { get; set; }
        public string[] Authors { get; set; }
        public string[] Owners { get; set; }
        public Uri IconUrl { get; set; }
        public Uri LicenseUrl { get; set; }
        public Uri ProjectUrl { get; set; }


        public void IncrementDownloadCount()
        {
            DownloadCount++;
        }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences { get; set; }

        public IEnumerable<IPackageFile> GetFiles()
        {
            return new List<IPackageFile>();
        }

        public Stream GetStream()
        {
            return Stream.Null;
        }

        public IEnumerable<System.Runtime.Versioning.FrameworkName> GetSupportedFrameworks()
        {
            return new List<FrameworkName>();
        }

        IEnumerable<string> IPackageMetadata.Authors
        {
            get { return Authors; }
        }

        public IEnumerable<PackageDependencySet> DependencySets { get; set; }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies
        { get; set; }

        public Version MinClientVersion
        { get; set; }

        IEnumerable<string> IPackageMetadata.Owners
        {
            get { return Owners; }
        }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences
        { get; set; }

        string IPackageName.Id
        {
            get
            {
                return PackageId;
            }
        }

        public Uri ReportAbuseUrl
        { get; set; }
    }
}
