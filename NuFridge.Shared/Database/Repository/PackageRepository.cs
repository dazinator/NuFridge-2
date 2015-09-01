using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NuFridge.Shared.Database.Model;
using NuGet;
using Palmer;
using Constants = NuFridge.Shared.Server.Constants;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageRepository : BaseRepository<InternalPackage>, IPackageRepository
    {
        private const string TableName = "Package";

        private const string GetAllPackagesStoredProcCommand = "NuFridge.GetAllPackages @feedId";
        private const string GetLatestPackagesStoredProcCommand = "NuFridge.GetLatestPackages @feedId, @includePrerelease, @partialId";
        private const string GetUniquePackageCountStoredProcCommand = "NuFridge.GetUniquePackageCount @feedId";
        private const string GetVersionsOfPackageStoredProcCommand = "NuFridge.GetVersionsOfPackage @feedId, @includePrerelease, @packageId";
        private const string GetPackageStoredProcCommand = "NuFridge.GetPackage @feedId, @packageId, @versionMajor, @versionMinor, @versionBuild, @versionRevision, @versionSpecial";

        public PackageRepository() : base(TableName)
        {

        }

        public IEnumerable<InternalPackage> GetAllPackagesForFeed(int feedId)
        {
            return Query<InternalPackage>(GetAllPackagesStoredProcCommand, new { feedId });
        }

        public IEnumerable<PackageUpload> GetLatestUploads(int feedId)
        {
            return
                Query<PackageUpload>(
                    $"SELECT TOP(10) [Id], [Version], [Published] FROM [NuFridge].[{TableName}] WITH(NOLOCK) WHERE FeedId = @feedId ORDER BY Published DESC",
                    new { feedId });
        }


        public void Delete(IEnumerable<int> ids)
        {
            ThrowIfReadOnly();

            Delete(ids, "Id");
        }

        public int GetCount(int feedId)
        {
            return RecordCount(true, $"WHERE FeedId = {feedId}");
        }

        public int GetUniquePackageIdCount(int feedId)
        {
            return Query<int>(GetUniquePackageCountStoredProcCommand, new { feedId }).Single();
        }

        public IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId)
        {
            return Query<InternalPackage>(GetLatestPackagesStoredProcCommand, new { feedId, includePrerelease, partialId });
        }

        public IEnumerable<InternalPackage> GetVersionsOfPackage(int? feedId, bool includePrerelease, string packageId)
        {
            return Query<InternalPackage>(GetVersionsOfPackageStoredProcCommand, new { feedId, includePrerelease, packageId });
        }

        public void Insert(InternalPackage package)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
            {
                using (var connection = GetConnection())
                {
                    package.PrimaryId = connection.Insert<int>(package);
                }
            });
        }

        public void Update(InternalPackage package)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        connection.Update(package);
                    }
                });
        }

        public InternalPackage GetPackage(int? feedId, string packageId, SemanticVersion version)
        {
            return Query<InternalPackage>(GetPackageStoredProcCommand,
                new
                {
                    feedId,
                    packageId,
                    versionMajor = version.Version.Major,
                    versionMinor = version.Version.Minor,
                    versionBuild = version.Version.Build,
                    versionRevision = version.Version.Revision,
                    versionSpecial = version.SpecialVersion
                }).FirstOrDefault();
        }

        public IEnumerable<InternalPackage> GetAllPackagesWithoutAHashOrSize()
        {
            return Query<InternalPackage>($"SELECT * FROM [NuFridge].[{TableName}] WITH(NOLOCK) WHERE Hash = '' OR PackageSize = 0");
        }
    }

    public interface IPackageRepository
    {
        IEnumerable<InternalPackage> GetAllPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        int GetCount(int feedId);
        int GetCount(bool noLock);
        int GetUniquePackageIdCount(int feedId);
        IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId);
        IEnumerable<InternalPackage> GetVersionsOfPackage(int? feedId, bool includePrerelease, string packageId);
        void Insert(InternalPackage package);
        void Update(InternalPackage package);
        void Delete(InternalPackage package);
        InternalPackage GetPackage(int? feedId, string packageId, SemanticVersion version);
        IEnumerable<InternalPackage> GetAllPackagesWithoutAHashOrSize();
        IEnumerable<PackageUpload> GetLatestUploads(int feedId);
    }
}