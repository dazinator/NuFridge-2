using System;
using Dapper;
using NuFridge.Shared.Database.Repository;
using NuGet;

namespace NuFridge.Shared.Database.Model
{
    [Table("PackageDownload", Schema = "NuFridge")]
    public class PackageDownload : IPackageDownload
    {
        [Key]
        public long Id { get; set; }

        public int FeedId { get; set; }
        public string PackageId { get; set; }
        public DateTime DownloadedAt { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }

        public int VersionMajor { get; set; }

        public int VersionMinor { get; set; }

        public int VersionBuild { get; set; }

        public int VersionRevision { get; set; }

        public string VersionSpecial { get; set; }

        [System.ComponentModel.DataAnnotations.Editable(false)]
        public string Version
        {
            get
            {
                return new SemanticVersion(new Version(VersionMajor, VersionMinor, VersionBuild, VersionRevision), VersionSpecial).ToString();
            }
            set
            {
                var semanticVersion = SemanticVersion.Parse(value);
                VersionBuild = semanticVersion.Version.Build;
                VersionMinor = semanticVersion.Version.Minor;
                VersionMajor = semanticVersion.Version.Major;
                VersionRevision = semanticVersion.Version.Revision;
                VersionSpecial = semanticVersion.SpecialVersion;
            }
        }
    }

    public interface IPackageDownload
    {
        long Id { get; set; }

        int FeedId { get; set; }
        string PackageId { get; set; }
        DateTime DownloadedAt { get; set; }
        string UserAgent { get; set; }


        int VersionMajor { get; set; }

        int VersionMinor { get; set; }
        int VersionBuild { get; set; }
        int VersionRevision { get; set; }
        string VersionSpecial { get; set; }
        string Version { get; set; }
    }
}