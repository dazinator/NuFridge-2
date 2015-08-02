using System;
using System.Data;
using System.Linq;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Model.Mappings
{
    public class InternalPackageMap : EntityMapping<IInternalPackage>
    {
        public static string GetPackageTable(int feedId)
        {
            return $"Package_{feedId}";
        }

        public InternalPackageMap()
        {
            TableName = "Package";

            Column(m => m.Description);
            Column(m => m.Hash);
            IdColumn = new ColumnMapping("Id", DbType.Int32, new PropertyReaderWriterDecorator(new DelegateReaderWriter<IInternalPackage, int>(target => target.PrimaryId, (package, i) => package.PrimaryId = i)));
            VirtualColumn("PackageId", DbType.String, package => package.Id, (package, s) => package.Id = s, 4000);
            Column(m => m.Published);
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

            Column(m => m.Created);
            Column(m => m.SupportedFrameworks);
            Column(m => m.Dependencies);
            Column(m => m.DevelopmentDependency);
            Column(m => m.VersionDownloadCount);
            Column(m => m.Language);
            Column(m => m.ReportAbuseUrl);
        }
    }
}
