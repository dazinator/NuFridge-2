namespace NuFridge.Shared.Server.Storage
{
    public interface IInitializeRelationalStore
    {
        void Initialize(IStore store);
    }
}
