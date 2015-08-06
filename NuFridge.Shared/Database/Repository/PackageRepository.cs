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
        private const string GetLatestPackagesStoredProcCommand = "NuFridge.GetLatestPackages @feedId";
        private const string GetUniquePackageCountStoredProcCommand = "NuFridge.GetUniquePackageCount @feedId";

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
            return Query<int>(GetUniquePackageCountStoredProcCommand, new { feedId }).Single();
        }

        public IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId)
        {
            return Query<InternalPackage>(GetLatestPackagesStoredProcCommand, new { feedId, includePrerelease, partialId });
        }
    }

    public interface IPackageRepository
    {
        IEnumerable<InternalPackage> GetPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        int GetCount(int feedId);
        int GetUniquePackageIdCount(int feedId);
        IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId);
    }
}