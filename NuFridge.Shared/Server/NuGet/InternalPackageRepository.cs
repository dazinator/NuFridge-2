using System;
using System.IO;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuFridge.Shared.Server.Storage;
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

        public override bool SupportsPrereleasePackages => true;

        public InternalPackageRepository(Func<int, PackageIndex> packageIndex, Func<int, IPackagePathResolver> packageResolver, Func<int, IFileSystem> fileSystem, SymbolSource symbolSource, IFrameworkNamesManager frameworkNamesManager, int feedId) : base(packageResolver(feedId), fileSystem(feedId))
        {
            _symbolSource = symbolSource;
            _packageIndex = packageIndex(feedId);
            _frameworkNamesManager = frameworkNamesManager;
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

        public void IncrementDownloadCount(IInternalPackage package)
        {
            _packageIndex.IncrementDownloadCount(package);
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
            IFeedConfiguration config;

            using (var dbContext = new DatabaseContext())
            {
                //Get the config file needed for later
                config = dbContext.FeedConfigurations.FirstOrDefault(fc => fc.FeedId == FeedId);

                //Delete the package
                _packageIndex.DeletePackage(internalPackage);
            }

            var filePath = GetPackageFilePath(internalPackage.Id, internalPackage.GetSemanticVersion());

            filePath = Path.Combine(FileSystem.Root, filePath);

            if (File.Exists(filePath))
            {
                IPackage package = FastZipPackage.FastZipPackage.Open(filePath, new CryptoHashProvider());

                base.RemovePackage(package);
            }

            //Remove the symbols once we know the transaction completed successfully
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

            var localPackage = InternalPackage.Create(FeedId, package, GetPackageFilePath);

             _packageIndex.AddPackage(localPackage);

            _frameworkNamesManager.Add(localPackage.SupportedFrameworks);
        }
    }
}