using System;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Database
{
    public class StoreFactory : IStoreFactory
    {
        private readonly Lazy<Store> _store;
        private readonly IHomeConfiguration _config;

        public Store Store => _store.Value;

        public StoreFactory(IHomeConfiguration config)
        {
            _config = config;
            _store = new Lazy<Store>(InitializeStore);
        }

        private Store InitializeStore()
        {
            return new Store(_config);
        }
    }
}
