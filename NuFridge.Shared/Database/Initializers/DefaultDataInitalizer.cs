using System;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Database.Initializers
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
        }
    }
}