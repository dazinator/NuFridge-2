using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class AdminUserInitializer : IInitializeStore
    {
        private readonly IUserService _userService;
        private readonly ILog _log = LogProvider.For<AdminUserInitializer>();

        public AdminUserInitializer(IUserService userService)
        {
            _userService = userService;
        }

        public void Initialize(IStore store, Action<string> updateStatusAction)
        {

            updateStatusAction("Checking if the administrator user exists");

            _log.Info("Checking if the administrator user exists.");

            _userService.CreateAdministratorUserIfNotExist();
        }
    }
}