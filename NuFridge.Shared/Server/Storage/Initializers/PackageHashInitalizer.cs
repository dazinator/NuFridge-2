using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
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

            List<IFeed> feeds;

            using (var transaction = store.BeginTransaction())
            {
                feeds = transaction.Query<IFeed>().ToList();
            }

            foreach (var feed in feeds)
            {
                _log.Debug("Updating packages in the " + feed.Name + " feed without a valid hash");

                var feedRepository = _packageRepositoryFactory.Create(feed.Id);

                using (var transaction = store.BeginTransaction())
                {
                    var packages =
                        transaction.Query<IInternalPackage>()
                            .Where("FeedId = @feedId AND Hash = ''")
                            .Parameter("feedId", feed.Id)
                            .ToList();

                    object sync = new object();

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

                            lock (sync)
                            {
                                transaction.Update(internalPackage);
                            }
                        }
                    });

                    transaction.Commit();
                }
            }
        }
    }
}