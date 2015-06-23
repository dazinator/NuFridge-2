namespace NuFridge.Shared.Server.Storage
{
    public interface IDatabaseMigrator
    {
        void Migrate(IStore store);
    }
}
