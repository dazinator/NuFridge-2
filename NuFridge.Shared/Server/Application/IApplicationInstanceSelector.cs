using System;

namespace NuFridge.Shared.Server.Application
{
    public interface IApplicationInstanceSelector
    {
        ApplicationInstanceRecord Current { get; }
        event Action Loaded;
        void LoadInstance();
    }
}