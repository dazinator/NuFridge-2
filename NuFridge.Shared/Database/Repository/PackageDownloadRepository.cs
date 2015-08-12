using System.Collections.Generic;
using Dapper;
using NuFridge.Shared.Database.Model;

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
            using (var connection = GetConnection())
            {
                packageDownload.Id = connection.Insert<int>(packageDownload);
            }
        }

        public IEnumerable<PackageDownload> GetLatestDownloads(int feedId)
        {
            return
                Query<PackageDownload>(
                    $"SELECT TOP(5) * FROM [NuFridge].[{TableName}] WHERE FeedId = @feedId ORDER BY [DownloadedAt] DESC",
                    new {feedId});
        }
    }

    public interface IPackageDownloadRepository
    {
        void Insert(PackageDownload packageDownload);
        IEnumerable<PackageDownload> GetLatestDownloads(int feedId);
    }
}