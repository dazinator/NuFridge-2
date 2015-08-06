using System.Collections.Generic;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;

        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public IEnumerable<IInternalPackage> GetPackagesForFeed(int feedId)
        {
            return _packageRepository.GetPackagesForFeed(feedId);
        }

        public int GetCount(int feedId)
        {
            return _packageRepository.GetCount(feedId);
        }

        public int GetUniquePackageIdCount(int feedId)
        {
            return _packageRepository.GetUniquePackageIdCount(feedId);
        }

        public void Delete(IEnumerable<int> ids)
        {
            _packageRepository.Delete(ids);
        }
    }

    public interface IPackageService
    {
        IEnumerable<IInternalPackage> GetPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        int GetCount(int feedId);
        int GetUniquePackageIdCount(int feedId);
    }
}