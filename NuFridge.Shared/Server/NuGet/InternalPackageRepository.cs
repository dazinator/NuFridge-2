using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class InternalPackageRepository : LocalPackageRepository, IInternalPackageRepository, IDisposable
    {
        public int FeedId { get; private set; }

        private readonly PackageIndex _packageIndex;

        private readonly object _fileLock = new object();


        public InternalPackageRepository(Func<int, PackageIndex> packageIndex, Func<int, IPackagePathResolver> packageResolver, Func<int, IFileSystem> fileSystem, int feedId) : base(packageResolver(feedId), fileSystem(feedId))
        {
            _packageIndex = packageIndex(feedId);
            FeedId = feedId;
        }

        public Stream GetRawContents(IInternalPackage package)
        {
            lock (_fileLock)
            {
                return FileSystem.OpenFile(GetPackageFilePath(package.PackageId, package.GetSemanticVersion()));
            }
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            return _packageIndex.GetPackage(packageId, version);
        }

        public void IncrementDownloadCount(IInternalPackage package)
        {
            _packageIndex.IncrementDownloadCount((IInternalPackage)package);
        }

        public IEnumerable<IInternalPackage> GetVersions(ITransaction transaction, string packageId, bool allowPreRelease)
        {
            return _packageIndex.GetVersions(transaction, packageId, allowPreRelease);
        }

        public IEnumerable<IInternalPackage> GetWebPackages(ITransaction transaction, string filterType, string filterColumn, string filterValue, string orderType, string orderProperty, string searchTerm, string targetFramework, string includePrerelease)
        {
            return _packageIndex.GetWebPackages(transaction, filterType, filterColumn, filterValue, orderType, orderProperty, searchTerm, targetFramework, includePrerelease);
        }

        public List<IInternalPackage> GetPackagesContaining(string searchTerm, out int total, int skip = 0, int take = 30, bool allowPreRelease = true)
        {
                return _packageIndex.GetPackagesContaining(searchTerm, out total, skip, take, allowPreRelease);
        }

        public Stream GetPackageRaw( string packageId, SemanticVersion version)
        {
            IInternalPackage package = GetPackage(packageId, version);
            if (package == null)
                return null;
            return GetRawContents(package);
        }

        public void RemovePackage(IInternalPackage internalPackage)
        {
            var filePath = GetPackageFilePath(internalPackage.PackageId, internalPackage.GetSemanticVersion());

            filePath = Path.Combine(FileSystem.Root, filePath);

            IPackage package = FastZipPackage.FastZipPackage.Open(filePath, new CryptoHashProvider());

            RemovePackage(package);

            _packageIndex.DeletePackage(internalPackage);
        }

        public void AddPackage(IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion)
        {
          //  var packagePath = GetPackageFilePath(package.Id, package.Version);

            var existingPackage = GetPackage(package.Id, package.Version);

            if (existingPackage != null)
            {
                throw new Exception(
                    "A package with the same ID and version already exists. Overwriting packages is not enabled on this feed.");
            }

            base.AddPackage(package);

            _packageIndex.AddPackage(package, isAbsoluteLatestVersion, isLatestVersion);
        }

        public void Dispose()
        {
        }
    }
}
