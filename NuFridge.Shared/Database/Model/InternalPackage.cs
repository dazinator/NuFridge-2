using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Dapper;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuGet;

namespace NuFridge.Shared.Database.Model
{
    [DebuggerDisplay("{Id}: {Version} ({Title})")]
    [Dapper.Table("Package", Schema = "NuFridge")]
    //   [TrackChanges]
    public class InternalPackage : IInternalPackage
    {
        /// <summary>
        /// This is the primary key id
        /// </summary>
        [System.ComponentModel.DataAnnotations.Key]
        [Key]
        public int PrimaryId { get; set; }

        /// <summary>
        /// This is the package id
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = false)]
        public string Id { get; set; }

        public int FeedId { get; set; }

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

        public DateTime Published { get; set; }

        public string Dependencies { get; set; }

        public string Hash { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        [NotMapped]
        [System.ComponentModel.DataAnnotations.Editable(false)]
        public long Size { get; set; }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies { get; set; }

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

        public string CalculateHash()
        {
            return Hash;
        }


        public string SupportedFrameworks { get; set; }

        public IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            return (SupportedFrameworks ?? "").Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(VersionUtility.ParseFrameworkName).Distinct();
        }

        public static IInternalPackage Create(int feedId, IPackage package, Func<IInternalPackage, string> getPackageFilePath)
        {
            var newPackage = new InternalPackage
            {
                Id = package.Id,
                FeedId = feedId,
                Description = package.Description,
                LastUpdated = DateTime.UtcNow,
                ReleaseNotes = package.ReleaseNotes,
                Summary = package.Summary,
                Title = package.Title,
                DownloadCount = package.DownloadCount,
                Copyright = package.Copyright,
                IsPrerelease = !package.IsReleaseVersion(),
                Listed = package.IsListed(),
                RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                Tags = package.Tags,
                Language = package.Language,
                FrameworkAssemblies = package.FrameworkAssemblies,
                DependencySets = package.DependencySets,
                DevelopmentDependency = package.DevelopmentDependency
            };

            newPackage.SupportedFrameworks = string.Join("|", package.GetSupportedFrameworks().Select(VersionUtility.GetShortFrameworkName));

            newPackage.Published = package.Published.HasValue ? package.Published.Value.LocalDateTime : DateTime.Now;

            if (string.IsNullOrWhiteSpace(newPackage.Title))
            {
                newPackage.Title = package.Id;
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

            if (package is DataServicePackage)
            {
                string filePath = getPackageFilePath(newPackage);

                using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] hash = new CryptoHashProvider().CalculateHash(stream);

                    newPackage.Hash = Convert.ToBase64String(hash);

                    stream.Seek(0, SeekOrigin.Begin);
                    newPackage.Created = FastZipPackageBase.GetPackageCreatedDateTime(stream);

                    newPackage.Size = stream.Length;
                }
            }
            else
            {
                var zipPackage = package as FastZipPackage;
                if (zipPackage != null)
                {
                    var zip = zipPackage;
                    newPackage.Hash = Convert.ToBase64String(zip.Hash);
                    newPackage.Size = zip.Size;
                    newPackage.Created = zipPackage.Created.DateTime;
                }
            }

            return newPackage;
        }


        [NotMapped]
        [System.ComponentModel.DataAnnotations.Editable(false)]
        public IEnumerable<PackageDependencySet> DependencySets
        {
            get { return PackageDependencySetConverter.Parse((Dependencies ?? string.Empty).Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)); }
            set { Dependencies = string.Join("|", value.SelectMany(PackageDependencySetConverter.Flatten)); }
        }

        public bool RequireLicenseAcceptance { get; set; }

        public string Language { get; set; }

        public string Tags { get; set; }

        [NotMapped]
        [System.ComponentModel.DataAnnotations.Editable(false)]
        public string PackageHashAlgorithm { get; set; }

        [NotMapped]
        [System.ComponentModel.DataAnnotations.Editable(false)]
        public long PackageSize { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime Created { get; set; }

        [ReadOnly(true)]
        public bool IsAbsoluteLatestVersion { get; set; }

        [ReadOnly(true)]
        public bool IsLatestVersion { get; set; }

        public bool IsPrerelease { get; set; }

        public bool Listed { get; set; }

        [ReadOnly(true)]
        public int DownloadCount { get; set; }

        [ReadOnly(true)]
        public int VersionDownloadCount { get; set; }

        public bool DevelopmentDependency { get; set; }

        public string Authors { get; set; }

        public string Owners { get; set; }

        public string IconUrl { get; set; }

        public string LicenseUrl { get; set; }

        public string ProjectUrl { get; set; }

        public string ReportAbuseUrl { get; set; }

        public void IncrementDownloadCount()
        {
            DownloadCount++;
        }
    }
}