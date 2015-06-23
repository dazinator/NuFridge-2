using System;
using System.Data;
using System.Linq;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Model.Mappings
{
    public class InternalPackageMap : EntityMapping<InternalPackage>
    {
        public InternalPackageMap()
        {
            TableName = "Package";

            Column(m => m.Description);
            VirtualColumn("Hash", DbType.String, m => m.PackageHash, (package, s) => package.PackageHash = s);
            Column(m => m.PackageId);
            Column(m => m.Published);
            Column(m => m.FeedId);
            Column(m => m.ReleaseNotes);
            Column(m => m.DownloadCount);
            Column(m => m.Summary);
            Column(m => m.Title);
            Column(m => m.VersionMajor);
            Column(m => m.VersionMinor);
            Column(m => m.VersionBuild);
            Column(m => m.VersionRevision);
            Column(m => m.VersionSpecial);
            Column(m => m.IsAbsoluteLatestVersion);
            Column(m => m.IsLatestVersion);
            Column(m => m.Copyright);
            VirtualColumn("Version", DbType.String, m => m.Version.ToString(), (package, s) => package.Version = SemanticVersion.Parse(s));
            Column(m => m.IsPrerelease);
            Column(m => m.LastUpdated);
            VirtualColumn("LicenseUrl", DbType.String, LicenseUrlReader, LicenseUrlWriter, null, true);
            Column(m => m.Listed);
            VirtualColumn("ProjectUrl", DbType.String, ProjectUrlReader, ProjectUrlWriter);
            Column(m => m.RequireLicenseAcceptance);
            Column(m => m.Tags);
            VirtualColumn("Owners", DbType.String, m => string.Join(",", (m.Owners ?? new string[0])), (package, s) => package.Owners = (s ?? String.Empty).Split(','));
            VirtualColumn("IconUrl", DbType.String, IconUrlReader, IconUrlWriter, null, true);
            VirtualColumn("Authors", DbType.String, m => string.Join(",", m.Authors ?? new string[0]), (package, s) => package.Authors = (s ?? String.Empty).Split(','));
        }

        private void ProjectUrlWriter(InternalPackage internalPackage, string o)
        {
            if (!string.IsNullOrWhiteSpace(o))
            {
                internalPackage.ProjectUrl = new Uri(o);
            }
        }

        private string ProjectUrlReader(InternalPackage arg)
        {
            return arg.ProjectUrl != null ? arg.ProjectUrl.ToString() : null;
        }

        private void LicenseUrlWriter(InternalPackage internalPackage, string o)
        {
            if (!string.IsNullOrWhiteSpace(o))
            {
                internalPackage.LicenseUrl = new Uri(o);
            }
        }

        private string LicenseUrlReader(InternalPackage arg)
        {
            return arg.LicenseUrl != null ? arg.LicenseUrl.ToString() : null;
        }

        private void IconUrlWriter(InternalPackage internalPackage, string o)
        {
            if (!string.IsNullOrWhiteSpace(o))
            {
                internalPackage.IconUrl = new Uri(o);
            }
        }

        private string IconUrlReader(InternalPackage arg)
        {
            return arg.IconUrl != null ? arg.IconUrl.ToString() : null;
        }
    }
}
