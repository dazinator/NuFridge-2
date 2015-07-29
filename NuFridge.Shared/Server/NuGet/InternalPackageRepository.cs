using System;
using System.Collections.Generic;
using System.IO;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class InternalPackageRepository : LocalPackageRepository, IInternalPackageRepository, IDisposable
    {
        public int FeedId { get; private set; }

        private readonly PackageIndex _packageIndex;

        private readonly object _fileLock = new object();
        private readonly ILog _log = LogProvider.For<InternalPackageRepository>();

        public override bool SupportsPrereleasePackages => true;

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
            _packageIndex.IncrementDownloadCount(package);
        }

        public IEnumerable<IInternalPackage> GetVersions(ITransaction transaction, string packageId, bool allowPreRelease)
        {
            return _packageIndex.GetVersions(transaction, packageId, allowPreRelease);
        }



        public Stream GetPackageRaw(string packageId, SemanticVersion version)
        {
            IInternalPackage package = GetPackage(packageId, version);
            if (package == null)
                return null;
            return GetRawContents(package);
        }

        public void DeletePackage(IInternalPackage internalPackage)
        {
            var filePath = GetPackageFilePath(internalPackage.PackageId, internalPackage.GetSemanticVersion());

            filePath = Path.Combine(FileSystem.Root, filePath);

            IPackage package = FastZipPackage.FastZipPackage.Open(filePath, new CryptoHashProvider());

            base.RemovePackage(package);

            _packageIndex.DeletePackage(internalPackage);
        }

        public void RemovePackage(IInternalPackage internalPackage)
        {
            _packageIndex.UnlistPackage(internalPackage);
        }

        public void IndexPackage(IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion)
        {
            _packageIndex.AddPackage(package, isAbsoluteLatestVersion, isLatestVersion);
        }

        public void AddPackage(IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion)
        {
            try
            {
                base.AddPackage(package);
            }
            catch (IOException ex)
            {
                _log.ErrorException("There was an IO error adding the package to the packages folder. " + ex.Message, ex);

                var filePath = GetPackageFilePath(package);
                
                if (FileSystem.FileExists(filePath))
                {
                    var fullPath = FileSystem.GetFullPath(filePath);

                    _log.Info("Deleting the file at " + fullPath + " as it did not get copied to the packages folder correctly.");
                    FileSystem.DeleteFile(filePath);
                }
                throw;
            }
         

            IndexPackage(package, isAbsoluteLatestVersion, isLatestVersion);
        }

        public void Dispose()
        {
        }
    }
}