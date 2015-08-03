using System;

namespace NuFridge.Shared.Server.Application
{
    public class ApplicationInstanceSelector : IApplicationInstanceSelector
    {
        private readonly IApplicationInstanceStore _instanceStore;

        public ApplicationInstanceSelector(IApplicationInstanceStore instanceStore)
        {
            _instanceStore = instanceStore;
        }

        public ApplicationInstanceRecord Current { get; private set; }
        public event Action Loaded;

        public void LoadInstance()
        {
            ApplicationInstanceRecord instance = _instanceStore.GetInstance();
            if (instance == null)
                throw new Exception("Instance has not been created.");
            Load(instance);
        }

        private void Load(ApplicationInstanceRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("record");
            Current = record;
            OnLoaded();
        }

        protected virtual void OnLoaded()
        {
            Action action = Loaded;
            if (action == null)
                return;
            action();
        }
    }
}