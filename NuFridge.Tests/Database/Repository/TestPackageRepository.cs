using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuGet;
using IPackageRepository = NuFridge.Shared.Database.Repository.IPackageRepository;

namespace NuFridge.Tests.Database.Repository
{
    public class TestPackageRepository : IPackageRepository
    {
        private List<IInternalPackage> _packages = new List<IInternalPackage>();

        public void WithPackages(List<IInternalPackage> packages)
        {
            _packages = packages;
        }

        public IEnumerable<IInternalPackage> GetAllPackagesForFeed(int feedId)
        {
            return _packages.Where(pk => pk.FeedId == feedId);
        }

        public void Delete(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                _packages.Remove(_packages.FirstOrDefault(pk => pk.PrimaryId == id));
            }
        }

        public int GetCount(int feedId)
        {
            return _packages.Count(pk => pk.FeedId == feedId);
        }

        public int GetCount(bool noLock)
        {
            return _packages.Count();
        }

        public long GetUniquePackageIdCount(int feedId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IInternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IInternalPackage> GetVersionsOfPackage(int? feedId, bool includePrerelease, string packageId)
        {
            throw new NotImplementedException();
        }

        public void Insert(IInternalPackage package)
        {
          _packages.Add(package);
        }

        public void Update(IInternalPackage package)
        {
            _packages[_packages.FindIndex(fd => fd.PrimaryId == package.PrimaryId)] = package;
        }

        public void Delete(IInternalPackage package)
        {
            _packages.Remove(package);
        }

        public IInternalPackage GetPackage(int? feedId, string packageId, SemanticVersion version)
        {
            return
                _packages.FirstOrDefault(
                    pk =>
                        (!feedId.HasValue || pk.FeedId == feedId.Value) && pk.Id == packageId && pk.GetSemanticVersion() == version);
        }

        public IEnumerable<IInternalPackage> GetAllPackagesWithoutAHashOrSize()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PackageUpload> GetLatestUploads(int feedId)
        {
            throw new NotImplementedException();
        }

        public long GetPackageDownloadCount(int feedId)
        {
            throw new NotImplementedException();
        }
    }
}