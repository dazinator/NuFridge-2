using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class DefaultDataInitalizer : IInitializeStore
    {
        private readonly IUserService _userService;
        private readonly ILog _log = LogProvider.For<DefaultDataInitalizer>();

        public DefaultDataInitalizer(IUserService userService)
        {
            _userService = userService;
        }

        public void Initialize(IStore store, Action<string> updateStatusAction)
        {
            updateStatusAction("Checking for default records in the database");

            _log.Info("Checking if the administrator user exists.");
            _userService.CreateAdministratorUserIfNotExist();
        }
    }
}