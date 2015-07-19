using System;
using System.IO;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class AdminUserInitializer : IInitializeRelationalStore
    {
        private readonly IHomeConfiguration _home;

        public AdminUserInitializer(IHomeConfiguration home)
        {
            _home = home;
        }

        public void Initialize(IStore store)
        {
            using (ITransaction transaction = store.BeginTransaction())
            {
                var count = transaction.Query<User>().Count();

                if (count == 0)
                {
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