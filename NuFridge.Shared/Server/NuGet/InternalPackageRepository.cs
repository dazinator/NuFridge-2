using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using Hangfire;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class InternalPackageRepository : LocalPackageRepository, IInternalPackageRepository
    {
        public int FeedId { get; private set; }

        private readonly PackageIndex _packageIndex;
        private readonly SymbolSource _symbolSource;
        private readonly IStore _store;
        private readonly object _fileLock = new object();
        private readonly ILog _log = LogProvider.For<InternalPackageRepository>();
        private readonly IInternalPackageRepositoryFactory _factory;

        public override bool SupportsPrereleasePackages => true;

        public InternalPackageRepository(Func<int, PackageIndex> packageIndex, Func<int, IPackagePathResolver> packageResolver, Func<int, IFileSystem> fileSystem, SymbolSource symbolSource, IStore store, IInternalPackageRepositoryFactory factory, int feedId) : base(packageResolver(feedId), fileSystem(feedId))
        {
            _symbolSource = symbolSource;
            _store = store;
            _packageIndex = packageIndex(feedId);
            FeedId = feedId;
            _factory = factory;
        }

        public Stream GetRawContents(IInternalPackage package)
        {
            lock (_fileLock)
            {
                return FileSystem.OpenFile(GetPackageFilePath(package.Id, package.GetSemanticVersion()));
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
            IFeedConfiguration config;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                //Get the config file needed for later
                config = transaction.Query<IFeedConfiguration>()
                    .Where("FeedId = @feedId")
                    .Parameter("feedId", FeedId)
                    .First();

                //Delete the package
                _packageIndex.DeletePackage(transaction, internalPackage);

                //Commit the transaction
                transaction.Commit();
            }

            //Set the latest version columns for the package id versions
            SetNextLatestVersionPackages(internalPackage.Id);

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
            using (ITransaction transaction = _store.BeginTransaction())
            {
                //Unlist the package
                _packageIndex.UnlistPackage(transaction, internalPackage);

                //Commit the transaction
                transaction.Commit();
            }

            SetNextLatestVersionPackages(internalPackage.Id);
        }

        public void SetNextLatestVersionPackages(string packageId)
        {
            using (var context = new DatabaseContext(FeedId, _store))
            {
                InternalPackage latestAbsoluteVersionPackage;
                InternalPackage latestVersionPackage;

                var versionsOfPackage = GetNextLatestVersionPackages(context, packageId, out latestAbsoluteVersionPackage, out latestVersionPackage);

                foreach (var internalPackage in versionsOfPackage)
                {
                    if (internalPackage.IsAbsoluteLatestVersion || internalPackage.IsLatestVersion)
                    {
                        internalPackage.IsLatestVersion = false;
                        internalPackage.IsAbsoluteLatestVersion = false;
                    }
                }

                if (latestAbsoluteVersionPackage != null)
                {
                    latestAbsoluteVersionPackage.IsAbsoluteLatestVersion = true;
                }


                if (latestVersionPackage != null)
                {
                    latestVersionPackage.IsLatestVersion = true;
                }

                context.SaveChanges();
            }
        }


        private List<InternalPackage> GetNextLatestVersionPackages(DatabaseContext context, string packageId, out InternalPackage latestAbsoluteVersionPackage, out InternalPackage latestVersionPackage)
        {
            latestAbsoluteVersionPackage = null;
            latestVersionPackage = null;

            List<InternalPackage> allPackages = context.Packages.Where(pk => pk.Id.ToLower() == packageId.ToLower() && pk.Listed).ToList();

            var releasePackages = allPackages.Where(vp => !vp.IsPrerelease).ToList();

            if (releasePackages.Any())
            {
                releasePackages.Sort(
                    (package, internalPackage) =>
                        internalPackage.GetSemanticVersion().CompareTo(package.GetSemanticVersion()));

                latestVersionPackage = releasePackages.First();
            }

            if (allPackages.Any())
            {
                allPackages.Sort(
                    (package, internalPackage) =>
                        internalPackage.GetSemanticVersion().CompareTo(package.GetSemanticVersion()));

                latestAbsoluteVersionPackage = allPackages.First();
            }

            return allPackages;
        }

        public void IndexPackage(IPackage package)
        {
            var localPackage = InternalPackage.Create(FeedId, package);

            using (var transaction = _store.BeginTransaction())
            {
                _packageIndex.AddPackage(transaction, localPackage);
                transaction.Commit();
            }

            SetNextLatestVersionPackages(package.Id);


            _factory.AddFrameworkNames(localPackage.SupportedFrameworks);
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

            var localPackage = InternalPackage.Create(FeedId, package);

            using (var transaction = _store.BeginTransaction())
            {
                _packageIndex.AddPackage(transaction, localPackage);
                transaction.Commit();
            }

            SetNextLatestVersionPackages(package.Id);


            _factory.AddFrameworkNames(localPackage.SupportedFrameworks);
        }
    }
}