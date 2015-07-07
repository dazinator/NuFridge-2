using System;
using System.Data;

namespace NuFridge.Shared.Server.Storage
{
    public interface IProjectionMapper
    {
        TResult Map<TResult>(string prefix);

        void Read(Action<IDataReader> callback);
    }
}
