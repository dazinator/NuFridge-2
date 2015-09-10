using System;
using System.IO;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class InternalPackageRepository : LocalPackageRepository, IInternalPackageRepository
    {
        public int FeedId { get; }

        private readonly PackageIndex _packageIndex;
        private readonly SymbolSource _symbolSource;
        private readonly ILog _log = LogProvider.For<InternalPackageRepository>();
        private readonly IFrameworkNamesManager _frameworkNamesManager;
        private readonly IFeedConfigurationService _feedConfigurationService;

        public override bool SupportsPrereleasePackages => true;

        public InternalPackageRepository(Func<int, PackageIndex> packageIndex, Func<int, IPackagePathResolver> packageResolver, Func<int, IFileSystem> fileSystem, SymbolSource symbolSource, IFrameworkNamesManager frameworkNamesManager, IFeedConfigurationService feedConfigurationService, int feedId) : base(packageResolver(feedId), fileSystem(feedId))
        {
            _symbolSource = symbolSource;
            _packageIndex = packageIndex(feedId);
            _frameworkNamesManager = frameworkNamesManager;
            _feedConfigurationService = feedConfigurationService;
            FeedId = feedId;
        }

        public string GetPackageFilePath(IInternalPackage package)
        {
            var filePath = GetPackageFilePath(package.Id, package.GetSemanticVersion());

            return Path.Combine(FileSystem.Root, filePath);
        }

        public Stream GetRawContents(IInternalPackage package)
        {
            return FileSystem.OpenFile(GetPackageFilePath(package.Id, package.GetSemanticVersion()));
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            return _packageIndex.GetPackage(packageId, version);
        }

        public void IncrementDownloadCount(IInternalPackage package, string ipAddress, string userAgent)
        {
            _packageIndex.IncrementDownloadCount(package, ipAddress, userAgent);
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
            IFeedConfiguration config = _feedConfigurationService.FindByFeedId(FeedId);

            //Delete the package
            _packageIndex.DeletePackage(internalPackage);

            var filePath = GetPackageFilePath(internalPackage.Id, internalPackage.GetSemanticVersion());

            filePath = Path.Combine(FileSystem.Root, filePath);

            if (File.Exists(filePath))
            {
                IPackage package = FastZipPackage.FastZipPackage.Open(filePath, new CryptoHashProvider());

                base.RemovePackage(package);
            }

            _symbolSource.RemoveSymbolPackage(config.SymbolsDirectory, internalPackage.Id, internalPackage.Version);
        }

        public void RemovePackage(IInternalPackage internalPackage)
        {
            _packageIndex.UnlistPackage(internalPackage);
        }

        public void IndexPackage(IPackage package)
        {
            var localPackage = InternalPackage.Create(FeedId, package, GetPackageFilePath);

            _packageIndex.AddPackage(localPackage);

            _frameworkNamesManager.Add(localPackage.SupportedFrameworks);
        }

        public new void AddPackage(IPackage package)
        {
            if (string.IsNullOrWhiteSpace(package.Id) || package.Version == null)
            {
                throw new InvalidPackageMetadataException("The package being added does not have a valid Id or Version");
            }

            IInternalPackage existingPackage = GetPackage(package.Id, package.Version);

            if (existingPackage != null)
            {
                throw new PackageConflictException($"A package with the same ID and version already exists - {package.Id} v{package.Version}");
            }

            string path = GetPackageFilePath(package);

            try
            {
                using (Stream stream = package.GetStream())
                {
                    if (FileSystem.FileExists(path))
                    {
                        _log.Debug($"Overwriting package file at {path}");
                        FileSystem.DeleteFile(path);
                    }

                    FileSystem.AddFile(path, stream);
                }
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

            var localPackage = InternalPackage.Create(FeedId, package, GetPackageFilePath);

            _packageIndex.AddPackage(localPackage);
            _frameworkNamesManager.Add(localPackage.SupportedFrameworks);
        }
    }
}