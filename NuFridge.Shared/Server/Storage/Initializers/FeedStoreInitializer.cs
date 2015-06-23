using System.IO;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class FeedStoreInitializer : IInitializeRelationalStore
    {
        private readonly IHomeConfiguration _home;

        public FeedStoreInitializer(IHomeConfiguration home)
        {
            _home = home;
        }

        public void Initialize(IStore store)
        {
            //bool createdFeed;

            //using (ITransaction transaction = store.BeginTransaction())
            //{
            //    createdFeed = EnsureFeed(transaction);
            //    transaction.Commit();
            //}

            //if (createdFeed)
            //{
            //    using (ITransaction transaction = store.BeginTransaction())
            //    {
            //        EnsureConfig(transaction);
            //        transaction.Commit();
            //    }
            //}
        }

        private void EnsureConfig(ITransaction transaction)
        {
            if (transaction.Query<FeedConfiguration>().First() != null)
                return;

            Feed feed = transaction.Query<Feed>().First();

            var appFolder = _home.InstallDirectory;
            var feedFolder = Path.Combine(appFolder, @"Feeds", feed.Name);

            FeedConfiguration config = new FeedConfiguration
            {
                FeedId = feed.Id,
                PackagesDirectory = feedFolder
            };

            transaction.Insert(config); 
        }

        private bool EnsureFeed(ITransaction transaction)
        {
            if (transaction.Query<Feed>().First() != null)
                return false;

            Feed feed = new Feed
            {
                Name = "Default"
            };

            transaction.Insert(feed);
            return true;
        }
    }
}