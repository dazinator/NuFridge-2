using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace NuFridge.Shared.Database.Model
{
    [Dapper.Table("PackageDownload", Schema = "NuFridge")]
    public class PackageDownload : IPackageDownload
    {
        [Dapper.Key]
        public long Id { get; set; }

        public int FeedId { get; set; }
        public string PackageId { get; set; }
        public DateTime DownloadedAt { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }

        private SemanticVersion SemanticVersion { get; set; }
        private string _version { get; set; }

        public int VersionMajor { get; set; }

        public int VersionMinor { get; set; }

        public int VersionBuild { get; set; }

        public int VersionRevision { get; set; }

        public string VersionSpecial { get; set; }

        [Editable(false)]
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