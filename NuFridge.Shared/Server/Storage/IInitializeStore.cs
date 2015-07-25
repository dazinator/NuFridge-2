using System;

namespace NuFridge.Shared.Server.Storage
{
    public interface IInitializeStore
    {
        void Initialize(IStore store, Action<string> updateStatusAction);
    }
}
