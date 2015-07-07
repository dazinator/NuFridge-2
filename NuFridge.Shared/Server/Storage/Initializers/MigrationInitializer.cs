namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class MigrationInitializer : IInitializeRelationalStore
    {
        private readonly IDatabaseMigrator _migrator;

        public MigrationInitializer(IDatabaseMigrator migrator)
        {
            _migrator = migrator;
        }

        public void Initialize(IStore store)
        {
            _migrator.Migrate(store);
        }
    }
}
