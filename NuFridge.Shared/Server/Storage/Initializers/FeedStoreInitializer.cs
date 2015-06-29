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

       
    }
}