using System.Collections.Generic;
using NuFridge.Shared.Database.Model.Interfaces;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageRepository : BaseRepository<IInternalPackage>, IPackageRepository
    {
        private const string TableName = "Package";
        private const string GetStoredProcName = "NuFridge.GetAllPackages";

        public PackageRepository() : base(TableName)
        {
            
        }

        public IEnumerable<IInternalPackage> GetPackagesForFeed(int feedId)
        {
            return Query(GetStoredProcName, new {feedId});
        }

        public void Delete(IEnumerable<int> ids)
        {
            Delete(ids, "Id");
        }
    }

    public interface IPackageRepository
    {
        IEnumerable<IInternalPackage> GetPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
    }
}