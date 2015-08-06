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

        public IEnumerable<InternalPackage> GetAllPackagesForFeed(int feedId)
        {
            return _packageRepository.GetAllPackagesForFeed(feedId);
        }

        public void Delete(InternalPackage package)
        {
            _packageRepository.Delete(package);
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

        public IEnumerable<InternalPackage> GetVersionsOfPackage(int feedId, bool includePrerelease, string packageId)
        {
            return _packageRepository.GetVersionsOfPackage(feedId, includePrerelease, packageId);
        }

        public void Insert(InternalPackage package)
        {
            _packageRepository.Insert(package);
        }

        public void Update(InternalPackage package)
        {
            _packageRepository.Update(package);
        }

        public InternalPackage GetPackage(int feedId, string packageId, string version)
        {
            return _packageRepository.GetPackage(feedId, packageId, version);
        }

        public void Delete(IEnumerable<int> ids)
        {
            _packageRepository.Delete(ids);
        }
    }

    public interface IPackageService
    {
        IEnumerable<InternalPackage> GetAllPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        void Delete(InternalPackage package);
        int GetCount(int feedId);
        int GetUniquePackageIdCount(int feedId);
        IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId);
        IEnumerable<InternalPackage> GetVersionsOfPackage(int feedId, bool includePrerelease, string packageId);
        void Insert(InternalPackage package);
        void Update(InternalPackage package);
        InternalPackage GetPackage(int feedId, string packageId, string version);
    }
}