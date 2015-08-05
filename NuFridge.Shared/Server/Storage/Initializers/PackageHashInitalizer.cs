using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuGet;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class PackageHashInitalizer : IInitializeStore
    {
        private readonly ILog _log = LogProvider.For<AdminUserInitializer>();
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;

        public PackageHashInitalizer(IInternalPackageRepositoryFactory packageRepositoryFactory)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
        }

        public void Initialize(IStore store, Action<string> updateStatusAction)
        {
            updateStatusAction("Migrating existing packages");

            List<Feed> feeds;

            using (var dbContext = new DatabaseContext())
            {
                feeds = dbContext.Feeds.AsNoTracking().ToList();
            }

            foreach (var feed in feeds)
            {

                var feedRepository = _packageRepositoryFactory.Create(feed.Id);

                using (var dbContext = new DatabaseContext())
                {
                    var packages = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id).Where(pk => pk.FeedId == feed.Id && pk.Hash == "").ToList();

                    if (packages.Any())
                    {
                        _log.Debug("Updating packages in the " + feed.Name + " feed without a valid hash");

                        Parallel.ForEach(packages, internalPackage =>
                        {
                            var filePath = feedRepository.GetPackageFilePath(internalPackage);
                            if (File.Exists(filePath))
                            {
                                var localPackage = FastZipPackage.Open(filePath, new CryptoHashProvider());

                                using (Stream stream = localPackage.GetStream())
                                {
                                    byte[] hash = new CryptoHashProvider().CalculateHash(stream);

                                    internalPackage.Hash = Convert.ToBase64String(hash);

                                    stream.Seek(0, SeekOrigin.Begin);

                                    internalPackage.Size = stream.Length;
                                }
                            }
                        });
                    }

                    dbContext.SaveChanges();
                }
            }
        }
    }
}