using System;
using System.Data.SqlClient;
using System.Linq;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.ScriptProviders;
using NuFridge.Shared.Database.Scripts;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Database
{
    public class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly ILog _log = LogProvider.For<DatabaseMigrator>();
        public void Migrate(IStore store)
        {
            _log.Debug("Checking to see if the database is up-to-date.");

            DatabaseUpgradeResult namespaceMigratorResult = DeployChanges.To.SqlDatabase(store.ConnectionString)
                .WithScripts(new EmbeddedScriptAndCodeProvider(typeof (Store).Assembly,
                    s => s == typeof (_ScriptNamespaceMirgrator).FullName))
                .LogScriptOutput()
                .WithVariable("databaseName", new SqlConnectionStringBuilder(store.ConnectionString).InitialCatalog)
                .LogTo(new LogAdapter())
                .JournalToSqlTable("NuFridge", "Version")
                .WithTransactionPerScript()
                .Build()
                .PerformUpgrade();

            if (!namespaceMigratorResult.Successful)
                throw new Exception("Database namespace migration failed: " + namespaceMigratorResult.Error.GetErrorSummary(), namespaceMigratorResult.Error);

            DatabaseUpgradeResult databaseUpgradeResult = DeployChanges.To.SqlDatabase(store.ConnectionString)
                .WithScriptsEmbeddedInAssembly(typeof (Store).Assembly)
                .LogScriptOutput()
                .WithVariable("databaseName", new SqlConnectionStringBuilder(store.ConnectionString).InitialCatalog)
                .LogTo(new LogAdapter())
                .JournalToSqlTable("NuFridge", "Version")
                .WithTransactionPerScript()
                .Build()
                .PerformUpgrade();

            if (!databaseUpgradeResult.Successful)
                throw new Exception("Database migration failed: " + databaseUpgradeResult.Error.GetErrorSummary(), databaseUpgradeResult.Error);
        }
    }

    public class LogAdapter : IUpgradeLog
    {
        private readonly ILog _log = LogProvider.For<LogAdapter>();

        public void WriteInformation(string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format) || !format.ToCharArray().Any(char.IsLetterOrDigit))
                return;
            _log.InfoFormat(format.TrimEnd(), args);
        }

        public void WriteError(string format, params object[] args)
        {
            _log.ErrorFormat(format, args);
        }

        public void WriteWarning(string format, params object[] args)
        {
            _log.WarnFormat(format, args);
        }
    }
}