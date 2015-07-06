using System.Collections.Generic;

namespace NuFridge.Shared.Server.Application
{
    public interface IApplicationInstanceStore
    {
        ApplicationInstanceRecord GetInstance();
        void Save(ApplicationInstanceRecord record);
    }
}