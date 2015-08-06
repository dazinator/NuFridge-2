using System.Collections.Generic;
using System.Linq;
using Dapper;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageRepository : BaseRepository<InternalPackage>, IPackageRepository
    {
        private const string TableName = "Package";
        private const string GetAllPackagesStoredProcCommand = "NuFridge.GetAllPackages @feedId";
        private const string GetUniquePackageCount = "NuFridge.GetUniquePackageCount @feedId";

        public PackageRepository() : base(TableName)
        {
            
        }

        public IEnumerable<InternalPackage> GetPackagesForFeed(int feedId)
        {
            return Query<InternalPackage>(GetAllPackagesStoredProcCommand, new {feedId});
        }

        public void Delete(IEnumerable<int> ids)
        {
            Delete(ids, "Id");
        }

        public int GetCount(int feedId)
        {
            using (var connection = GetConnection())
            {
                return connection.RecordCount<InternalPackage>($"WHERE FeedId = {feedId}");
            }
        }

        public int GetUniquePackageIdCount(int feedId)
        {
            return Query<int>(GetUniquePackageCount, new { feedId }).Single();
        }
    }

    public interface IPackageRepository
    {
        IEnumerable<InternalPackage> GetPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        int GetCount(int feedId);
        int GetUniquePackageIdCount(int feedId);
    }
}