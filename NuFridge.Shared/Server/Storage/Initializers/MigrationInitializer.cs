using System;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class MigrationInitializer : IInitializeStore
    {
        private readonly IDatabaseMigrator _migrator;

        public MigrationInitializer(IDatabaseMigrator migrator)
        {
            _migrator = migrator;
        }

        public void Initialize(IStore store, Action<string> updateStatusAction)
        {
            updateStatusAction("Upgrading the database");

            _migrator.Migrate(store);
        }
    }
}
