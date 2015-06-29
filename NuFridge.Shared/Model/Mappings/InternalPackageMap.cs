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
            Column(m => m.Hash);
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
            Column(m => m.Version);
            Column(m => m.IsPrerelease);
            Column(m => m.LastUpdated);
            Column( m=> m.LicenseUrl);
            Column(m => m.Listed);
            Column(m => m.ProjectUrl);
            Column(m => m.RequireLicenseAcceptance);
            Column(m => m.Tags);
            Column(m => m.Owners);
            Column(m => m.IconUrl);
            Column(m => m.Authors);
        }
    }
}
