using System.Data;

namespace NuFridge.Shared.Server.Storage
{
    public interface IStore
    {
        string ConnectionString { get; }

        ITransaction BeginTransaction();

        ITransaction BeginTransaction(IsolationLevel isolationLevel);
    }
}
