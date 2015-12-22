using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using NuGet;

namespace NuFridge.Shared.Database.Model.Interfaces
{
    public interface IInternalPackage
    {
        bool IsLatestVersion { get; set; }
        bool IsAbsoluteLatestVersion { get; set; }

        int PrimaryId { get; set; }
        string Id { get; set; }
        string Version { get; set; }
        int FeedId { get; set; }
        string Description { get; set; }

        string ReleaseNotes { get; set; }

        DateTime Published { get; set; }

        string Title { get; set; }

        string Summary { get; set; }

        int VersionMajor { get; set; }
        int VersionMinor { get; set; }
        int VersionBuild { get; set; }
        int VersionRevision { get; set; }
        string VersionSpecial { get; set; }
        string MinClientVersion { get; set; }
        int DownloadCount { get; set; }

        bool IsReleaseVersion();

        long GetSize();
        IEnumerable<FrameworkName> GetSupportedFrameworks();
        bool RequireLicenseAcceptance { get; set; }
        string Language { get; set; }
        string Tags { get; set; }
        string PackageHashAlgorithm { get; set; }
        long PackageSize { get; set; }
        long Size { get; set; }
        DateTime LastUpdated { get; set; }
        DateTime Created { get; set; }
        bool IsPrerelease { get; set; }
        bool Listed { get; set; }
        string Dependencies { get; set; }
        int VersionDownloadCount { get; set; }
        bool DevelopmentDependency { get; set; }
        string Authors { get; set; }
        string Owners { get; set; }
        string IconUrl { get; set; }
        string LicenseUrl { get; set; }
        string Copyright { get; set; }
        string ProjectUrl { get; set; }
        string Hash { get; set; }
        string CalculateHash();
        void IncrementDownloadCount();
        SemanticVersion GetSemanticVersion();
        string ReportAbuseUrl { get; set; }
        string SupportedFrameworks { get; set; }
    }
}
