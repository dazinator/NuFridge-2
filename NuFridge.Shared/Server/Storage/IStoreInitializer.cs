namespace NuFridge.Shared.Server.Storage
{
    public interface IStoreInitializer
    {
        void Initialize();

        void Stop();
    }
}
