using System;
using System.Data.Entity;
using System.IO;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class AdminUserInitializer : IInitializeStore
    {
        private readonly ILog _log = LogProvider.For<AdminUserInitializer>();

        public void Initialize(IStore store, Action<string> updateStatusAction)
        {
            Database.SetInitializer<ReadOnlyDatabaseContext>(null);

            updateStatusAction("Checking if the administrator user exists");

            _log.Info("Checking if the administrator user exists.");

            using (ITransaction transaction = store.BeginTransaction())
            {
                var count = transaction.Query<User>().Count();

                if (count == 0)
                {
                    _log.Info("Creating the administrator user.");

                    User user = new User("administrator");
                    user.IsActive = true;
                    user.EmailAddress = "admin@nufridge.com";
                    user.LastUpdated = DateTime.Now;
                    user.SetPassword("password");
                    user.DisplayName = "Administrator";
                    
                    transaction.Insert(user);

                    transaction.Commit();
                }
            }
        }
    }
}