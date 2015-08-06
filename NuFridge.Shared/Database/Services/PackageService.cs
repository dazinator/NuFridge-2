using System.Collections.Generic;
using NuFridge.Shared.Database.Model;
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

        public IEnumerable<InternalPackage> GetPackagesForFeed(int feedId)
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

        public IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId)
        {
            return _packageRepository.GetLatestPackagesForFeed(feedId, includePrerelease, partialId);
        }

        public void Delete(IEnumerable<int> ids)
        {
            _packageRepository.Delete(ids);
        }
    }

    public interface IPackageService
    {
        IEnumerable<InternalPackage> GetPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        int GetCount(int feedId);
        int GetUniquePackageIdCount(int feedId);
        IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId);
    }
}