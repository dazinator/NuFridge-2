namespace NuFridge.Shared.Server.Storage
{
    public class StoreInitializer : IStoreInitializer
    {
        private readonly IStore _store;
        private readonly IInitializeRelationalStore[] _initializers;

        public StoreInitializer(IStore store, IInitializeRelationalStore[] initializers)
        {
            _store = store;
            _initializers = initializers;
        }

        public void Initialize()
        {
            foreach (IInitializeRelationalStore initializeRelationalStore in _initializers)
                initializeRelationalStore.Initialize(_store);
        }

        public void Stop()
        {
        }
    }
}
