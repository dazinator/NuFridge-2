using System;

namespace NuFridge.Shared.Database
{
    public interface IInitializeStore
    {
        void Initialize(IStore store, Action<string> updateStatusAction);
    }
}
