using System;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database
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
            System.Data.Entity.Database.SetInitializer<DatabaseContext>(null);

            foreach (IInitializeStore initializeRelationalStore in _initializers)
                initializeRelationalStore.Initialize(_store, updateStatusAction);
        }

        public void Stop()
        {
        }
    }
}
