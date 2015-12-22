using System.Collections.Generic;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Reporting;
using NuGet;
using IPackageRepository = NuFridge.Shared.Database.Repository.IPackageRepository;

namespace NuFridge.Shared.Database.Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepository _packageRepository;

        public PackageService(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public IEnumerable<IInternalPackage> GetAllPackagesForFeed(int feedId)
        {
            return _packageRepository.GetAllPackagesForFeed(feedId);
        }

        public void Delete(IInternalPackage package)
        {
            _packageRepository.Delete(package);
        }

        public int GetCount(int feedId)
        {
            return _packageRepository.GetCount(feedId);
        }

        public int GetCount()
        {
            return _packageRepository.GetCount(true);
        }

        public long GetPackageDownloadCount(int feedId)
        {
            return _packageRepository.GetPackageDownloadCount(feedId);
        }

        public long GetUniquePackageIdCount(int feedId)
        {
            return _packageRepository.GetUniquePackageIdCount(feedId);
        }

        public IEnumerable<IInternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId)
        {
            return _packageRepository.GetLatestPackagesForFeed(feedId, includePrerelease, partialId);
        }

        public IEnumerable<IInternalPackage> GetVersionsOfPackage(int? feedId, bool includePrerelease, string packageId)
        {
            return _packageRepository.GetVersionsOfPackage(feedId, includePrerelease, packageId);
        }

        public IEnumerable<IInternalPackage> GetAllPackagesWithoutAHashOrSize()
        {
            return _packageRepository.GetAllPackagesWithoutAHashOrSize();
        }

        public void Insert(IInternalPackage package)
        {
            _packageRepository.Insert(package);
        }

        public void Update(IInternalPackage package)
        {
            _packageRepository.Update(package);
        }

        public IInternalPackage GetPackage(int? feedId, string packageId, SemanticVersion version)
        {
            return _packageRepository.GetPackage(feedId, packageId, version);
        }

        public IEnumerable<PackageUpload> GetLatestUploads(int feedId)
        {
            return _packageRepository.GetLatestUploads(feedId);
        }

        public void Delete(IEnumerable<int> ids)
        {
            _packageRepository.Delete(ids);
        }
    }

    public interface IPackageService
    {
        IEnumerable<IInternalPackage> GetAllPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        void Delete(IInternalPackage package);
        int GetCount(int feedId);
        long GetUniquePackageIdCount(int feedId);
        IEnumerable<IInternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId);
        IEnumerable<IInternalPackage> GetVersionsOfPackage(int? feedId, bool includePrerelease, string packageId);
        IEnumerable<IInternalPackage> GetAllPackagesWithoutAHashOrSize(); 
        void Insert(IInternalPackage package);
        int GetCount();
        void Update(IInternalPackage package);
        IInternalPackage GetPackage(int? feedId, string packageId, SemanticVersion version);
        IEnumerable<PackageUpload> GetLatestUploads(int feedId);
        long GetPackageDownloadCount(int feedId);
    }
}