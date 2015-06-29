using System;
using System.Collections.Generic;
using NuGet;

namespace NuFridge.Shared.Model.Interfaces
{
    public interface IInternalPackage
    {
        string PackageId { get; }

        string Version { get; }

        string Description { get; }

        string ReleaseNotes { get; }

        DateTimeOffset? Published { get; }

        string Title { get; }

        string Summary { get; }

        int DownloadCount { get; set; }

        bool IsReleaseVersion();

        long GetSize();

        List<string> GetDependencies();

         string DisplayTitle { get; set; }
         bool RequireLicenseAcceptance { get; set; }
         string Language { get; set; }
         string Tags { get; set; }
         string PackageHashAlgorithm { get; set; }
         long PackageSize { get; set; }
         DateTime LastUpdated { get; set; }
         DateTime Created { get; set; }
         bool IsAbsoluteLatestVersion { get; set; }
         bool IsLatestVersion { get; set; }
         bool IsPrerelease { get; set; }
         bool Listed { get; set; }

         int VersionDownloadCount { get; set; }
         bool DevelopmentDependency { get; set; }
         float Score { get; set; }
         string Authors { get; set; }
         string Owners { get; set; }
         string IconUrl { get; set; }
         string LicenseUrl { get; set; }
         string Copyright { get; set; }
         string ProjectUrl { get; set; }

        string CalculateHash();
        void IncrementDownloadCount();
        SemanticVersion GetSemanticVersion();
        void SetSemanticVersion(SemanticVersion value);
    }
}
