using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class EntityFrameworkInitializer : IInitializeStore
    {
        private readonly ILog _log = LogProvider.For<EntityFrameworkInitializer>();

        public void Initialize(IStore store)
        {
            _log.Info("Initializing the database.");

            using (var dbContext = new DatabaseContext(store))
            {
                dbContext.Database.Initialize(false);
            }
        }
    }
}