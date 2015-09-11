using System;

namespace NuFridge.Shared.Application
{
    public interface IApplicationInstanceSelector
    {
        ApplicationInstanceRecord Current { get; }
        event Action Loaded;
        void LoadInstance();
    }
}