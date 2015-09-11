namespace NuFridge.Shared.Database
{
    public interface IDatabaseMigrator
    {
        void Migrate(IStore store);
    }
}
