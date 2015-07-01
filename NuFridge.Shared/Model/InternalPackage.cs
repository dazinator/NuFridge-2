using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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
    [Table("Package", Schema = "NuFridge")]
    [DebuggerDisplay("{PackageId}: {Version} ({Title})")]
    public class InternalPackage : IInternalPackage, IEntity
    {
        public InternalPackage()
        {
            IsAbsoluteLatestVersion = true;
        }

        [Key]
        public int Id { get; set; }

        public int FeedId { get; set; }

        public string PackageId { get; set; }

 
        private SemanticVersion SemanticVersion { get; set; }
        private string _version { get; set; }

        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                SemanticVersion = SemanticVersion.Parse(value);
                VersionBuild = SemanticVersion.Version.Build;
                VersionMinor = SemanticVersion.Version.Minor;
                VersionMajor = SemanticVersion.Version.Major;
                VersionRevision = SemanticVersion.Version.Revision;
                VersionSpecial = SemanticVersion.SpecialVersion;
            }
        }

        public SemanticVersion GetSemanticVersion()
        {
            return SemanticVersion;
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

        public string Hash { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

                  [NotMapped]
        public long Size { get; set; }

        public bool IsReleaseVersion()
        {
            var semVersion = GetSemanticVersion();
            if (semVersion != null)
            {
                return string.IsNullOrWhiteSpace(semVersion.SpecialVersion);
            }
            return false;
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
            return Hash;
        }

        public static IInternalPackage Create(int feedId, IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion)
        {
            var newPackage = new InternalPackage
             {
                 PackageId = package.Id,
                 FeedId = feedId,
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

                 Listed = true,

                 RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                 Tags = package.Tags,

             };

            if (string.IsNullOrWhiteSpace(newPackage.Title))
            {
                newPackage.Title = package.Id;
            }

            if (string.IsNullOrWhiteSpace(newPackage.DisplayTitle))
            {
                newPackage.DisplayTitle = package.Id;
            }

            newPackage.Version = package.Version.ToString();

            if (package.ProjectUrl != null)
            {
                newPackage.ProjectUrl = package.ProjectUrl.ToString();
            }

            if (package.IconUrl != null)
            {
                newPackage.IconUrl = package.IconUrl.ToString();
            }

            if (package.LicenseUrl != null)
            {
                newPackage.LicenseUrl = package.LicenseUrl.ToString();
            }

            if (package.ReportAbuseUrl != null)
            {
                newPackage.ReportAbuseUrl = package.ReportAbuseUrl.ToString();
            }


            if (package.Owners != null)
            {
                newPackage.Owners = string.Join(",", package.Owners);
            }

            if (package.Authors != null)
            {
                newPackage.Authors = string.Join(",", package.Authors);
            }

            using (Stream stream = package.GetStream())
            {
                newPackage.Hash = HashCalculator.Hash(stream);
                newPackage.Size = stream.Length;
            }

            return newPackage;
        }


        public string Name
        {
            get { return PackageId; }
        }

           [NotMapped]
        public string DisplayTitle { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
           [NotMapped]
        public string Language { get; set; }
        public string Tags { get; set; }
           [NotMapped]
        public string PackageHashAlgorithm { get; set; }
           [NotMapped]
        public long PackageSize { get; set; }
        public DateTime LastUpdated { get; set; }
           [NotMapped]
        public DateTime Created { get; set; }
        public bool IsAbsoluteLatestVersion { get; set; }
        public bool IsLatestVersion { get; set; }
        public bool IsPrerelease { get; set; }
        public bool Listed { get; set; }
        public int DownloadCount { get; set; }
           [NotMapped]
        public int VersionDownloadCount { get; set; }
           [NotMapped]
        public bool DevelopmentDependency { get; set; }
        [NotMapped]
        public float Score { get; set; }
        public string Authors { get; set; }
        public string Owners { get; set; }
        public string IconUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }


        public void IncrementDownloadCount()
        {
            DownloadCount++;
        }
           [NotMapped]
        public string ReportAbuseUrl { get; set; }
    }
}
