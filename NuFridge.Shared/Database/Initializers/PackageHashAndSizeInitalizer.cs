using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.NuGet.Packaging;
using NuFridge.Shared.NuGet.Repository;
using NuGet;

namespace NuFridge.Shared.Database.Initializers
{
    public class PackageHashAndSizeInitalizer : IInitializeStore
    {
        private readonly ILog _log = LogProvider.For<PackageHashAndSizeInitalizer>();
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IPackageService _packageService;
        private readonly object _sync = new object();
        private readonly List<IInternalPackageRepository> _packageRepositories = new List<IInternalPackageRepository>();

        public PackageHashAndSizeInitalizer(IInternalPackageRepositoryFactory packageRepositoryFactory, IPackageService packageService)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _packageService = packageService;
        }

        public void Initialize(IStore store, Action<string> updateStatusAction)
        {
            IEnumerable<InternalPackage> packages = _packageService.GetAllPackagesWithoutAHashOrSize().ToList();

            if (!packages.Any())
            {
                return;
            }

            updateStatusAction("Migrating existing packages");

            _log.Info($"{packages.Count()} packages need to be updated as they are missing a hash/size. Please wait.");

            Parallel.ForEach(packages, package =>
            {
                var repo = GetPackageRepository(package.FeedId);

                var filePath = repo.GetPackageFilePath(package);

                if (File.Exists(filePath))
                {
                    var localPackage = FastZipPackage.Open(filePath, new CryptoHashProvider());

                    using (Stream stream = localPackage.GetStream())
                    {
                        byte[] hash = new CryptoHashProvider().CalculateHash(stream);

                        package.Hash = Convert.ToBase64String(hash);

                        stream.Seek(0, SeekOrigin.Begin);

                        package.PackageSize = stream.Length;
                    }

                    _packageService.Update(package);
                }
                else
                {
                    _log.Warn($"The {package.Id} v{package.Version} file is missing from the {package.FeedId} feed. It's hash/size can not be updated.");
                }
            });
        }

        private IInternalPackageRepository GetPackageRepository(int feedId)
        {
            lock (_sync)
            {
                IInternalPackageRepository repo = _packageRepositories.FirstOrDefault(pr => pr.FeedId == feedId);
                if (repo == null)
                {
                    repo = _packageRepositoryFactory.Create(feedId);
                    _packageRepositories.Add(repo);
                }

                return repo;
            }
        }
    }
}