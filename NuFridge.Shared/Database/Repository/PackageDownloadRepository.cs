using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server;
using Palmer;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageDownloadRepository : BaseRepository<PackageDownload>, IPackageDownloadRepository
    {
        private const string TableName = "PackageDownload";

        public PackageDownloadRepository() : base(TableName)
        {
            
        }

        public void Insert(PackageDownload packageDownload)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        packageDownload.Id = connection.Insert<int>(packageDownload);
                    }
                });
        }

        public IEnumerable<PackageDownload> GetLatestDownloads(int feedId)
        {
            return
                Query<PackageDownload>(
                    $"SELECT TOP(10) * FROM [NuFridge].[{TableName}] WITH(NOLOCK) WHERE FeedId = @feedId ORDER BY [DownloadedAt] DESC",
                    new {feedId});
        }
    }

    public interface IPackageDownloadRepository
    {
        void Insert(PackageDownload packageDownload);
        IEnumerable<PackageDownload> GetLatestDownloads(int feedId);
        int GetCount(bool nolock);
    }
}