using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuGet;

namespace NuFridge.Shared.Server.Web.OData
{
    [EntityPropertyMapping("Id", SyndicationItemProperty.Title, SyndicationTextContentKind.Plaintext, keepInContent: false)]
    [EntityPropertyMapping("Authors", SyndicationItemProperty.AuthorName, SyndicationTextContentKind.Plaintext, keepInContent: false)]
    [EntityPropertyMapping("LastUpdated", SyndicationItemProperty.Updated, SyndicationTextContentKind.Plaintext, keepInContent: false)]
    [EntityPropertyMapping("Summary", SyndicationItemProperty.Summary, SyndicationTextContentKind.Plaintext, keepInContent: false)]
    public class ODataPackage : IEquatable<ODataPackage>
    {
        public ODataPackage()
        {
        }

        public ODataPackage(IInternalPackage package)
        {

            Version = package.Version;
            Authors = package.Authors;
            Owners = package.Owners;
            IconUrl = package.IconUrl;
            LicenseUrl = package.LicenseUrl;
            ProjectUrl = package.ProjectUrl;
            Dependencies = package.Dependencies;

            Id = package.Id;
            Title = package.Title;
            DisplayTitle = package.Title;
            RequireLicenseAcceptance = package.RequireLicenseAcceptance;
            Description = package.Description;
            Summary = package.Summary;
            ReleaseNotes = package.ReleaseNotes;
            Language = package.Language;
            Tags = package.Tags;
            PackageHash = package.Hash;
            PackageHashAlgorithm = package.PackageHashAlgorithm;
            PackageSize = package.PackageSize;
            LastUpdated = package.LastUpdated;
            Published = package.Published;
            Created = package.Created;
            IsAbsoluteLatestVersion = package.IsAbsoluteLatestVersion;
            IsLatestVersion = package.IsLatestVersion;
            IsPrerelease = package.IsPrerelease;
            Listed = package.Listed;
            DownloadCount = package.DownloadCount;
            VersionDownloadCount = package.VersionDownloadCount;
            DevelopmentDependency = package.DevelopmentDependency;
        }

        public string Id { get; set; }

        public string Version { get; set; }

        public string Title { get; set; }

        public string DisplayTitle { get; set; }

        public string Authors { get; set; }

        public string Owners { get; set; }

        public string IconUrl { get; set; }

        public string LicenseUrl { get; set; }

        public string ProjectUrl { get; set; }

        public int DownloadCount { get; set; }

        public bool RequireLicenseAcceptance { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public string ReleaseNotes { get; set; }

        public string Language { get; set; }

        public DateTime Created { get; set; }

        public DateTime Published { get; set; }

        public DateTime LastUpdated { get; set; }

        public string Dependencies { get; set; }

        public string PackageHash { get; set; }

        public string PackageHashAlgorithm { get; set; }

        public long PackageSize { get; set; }

        public string Copyright { get; set; }

        public string Tags { get; set; }

        public bool IsAbsoluteLatestVersion { get; set; }

        public bool IsLatestVersion { get; set; }

        public bool IsPrerelease { get; set; }

        public bool Listed { get; set; }

        public bool DevelopmentDependency { get; set; }

        public int VersionDownloadCount { get; set; }

        public bool Equals(ODataPackage other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Equals(Id, other.Id) && Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(obj, null) && ReferenceEquals(this, obj) || Equals(obj as ODataPackage);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() * 37 + Version.GetHashCode() * 11;
        }
    }
}
