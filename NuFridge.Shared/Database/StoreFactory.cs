using System;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Database
{
    public class StoreFactory : IStoreFactory
    {
        private readonly Lazy<IStore> _store;
        private readonly IHomeConfiguration _config;

        public IStore Store => _store.Value;

        public StoreFactory(IHomeConfiguration config)
        {
            _config = config;
            _store = new Lazy<IStore>(InitializeStore);
        }

        private IStore InitializeStore()
        {
            return new Store(_config);
        }
    }
}
