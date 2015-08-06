using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
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
        [Key]
        [Column("Id")]
        public int PrimaryId { get; set; }

        /// <summary>
        /// This is the package id
        /// </summary>
        [Column("PackageId")]
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

        [SkipTracking]
        public int VersionMajor { get; set; }

        [SkipTracking]
        public int VersionMinor { get; set; }

        [SkipTracking]
        public int VersionBuild { get; set; }

        [SkipTracking]
        public int VersionRevision { get; set; }

        [SkipTracking]
        public string VersionSpecial { get; set; }

        [SkipTracking]
        public string Description { get; set; }

        [SkipTracking]
        public string ReleaseNotes { get; set; }

        [SkipTracking]
        public string Copyright { get; set; }

        [SkipTracking]
        public DateTime Published { get; set; }

        [SkipTracking]
        public string Dependencies { get; set; }

        [SkipTracking]
        public string Hash { get; set; }

        [SkipTracking]
        public string Title { get; set; }

        [SkipTracking]
        public string Summary { get; set; }

        [NotMapped]
        [SkipTracking]
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

        [SkipTracking]
        public string SupportedFrameworks { get; set; }

        public IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            return (SupportedFrameworks ?? "").Split(new [] {"|"}, StringSplitOptions.RemoveEmptyEntries).Select(VersionUtility.ParseFrameworkName).Distinct();
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
                }
            }

            return newPackage;
        }



        [NotMapped]
        public IEnumerable<PackageDependencySet> DependencySets
        {
            get { return PackageDependencySetConverter.Parse((Dependencies ?? string.Empty).Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)); }
            set { Dependencies = string.Join("|", value.SelectMany(PackageDependencySetConverter.Flatten)); }
        }

        [SkipTracking]
        public bool RequireLicenseAcceptance { get; set; }

        [SkipTracking]
        public string Language { get; set; }

        [SkipTracking]
        public string Tags { get; set; }

        [NotMapped]
        [SkipTracking]
        public string PackageHashAlgorithm { get; set; }

        [NotMapped]
        [SkipTracking]
        public long PackageSize { get; set; }

        [SkipTracking]
        public DateTime LastUpdated { get; set; }

        [SkipTracking]
        public DateTime Created { get; set; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool IsAbsoluteLatestVersion { get; set; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool IsLatestVersion { get; set; }

        [SkipTracking]
        public bool IsPrerelease { get; set; }

        [SkipTracking]
        public bool Listed { get; set; }

        [SkipTracking]
        public int DownloadCount { get; set; }

        [SkipTracking]
        public int VersionDownloadCount { get; set; }

        [SkipTracking]
        public bool DevelopmentDependency { get; set; }

        [SkipTracking]
        public string Authors { get; set; }

        [SkipTracking]
        public string Owners { get; set; }

        [SkipTracking]
        public string IconUrl { get; set; }

        [SkipTracking]
        public string LicenseUrl { get; set; }

        [SkipTracking]
        public string ProjectUrl { get; set; }


        public void IncrementDownloadCount()
        {
            DownloadCount++;
        }

        [SkipTracking]
        public string ReportAbuseUrl { get; set; }
    }
}
