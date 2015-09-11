using System;

namespace NuFridge.Shared.Database
{
    public interface IStoreInitializer
    {
        void Initialize(Action<string> updateStatusAction);

        void Stop();
    }
}
