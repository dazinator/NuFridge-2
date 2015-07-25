using System;

namespace NuFridge.Shared.Server.Storage
{
    public interface IStoreInitializer
    {
        void Initialize(Action<string> updateStatusAction);

        void Stop();
    }
}
