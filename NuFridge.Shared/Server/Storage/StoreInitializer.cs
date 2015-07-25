using System;

namespace NuFridge.Shared.Server.Storage
{
    public class StoreInitializer : IStoreInitializer
    {
        private readonly IStore _store;
        private readonly IInitializeStore[] _initializers;

        public StoreInitializer(IStore store, IInitializeStore[] initializers)
        {
            _store = store;
            _initializers = initializers;
        }

        public void Initialize(Action<string> updateStatusAction)
        {
            updateStatusAction("Upgrading database");

            foreach (IInitializeStore initializeRelationalStore in _initializers)
                initializeRelationalStore.Initialize(_store);
        }

        public void Stop()
        {
        }
    }
}
