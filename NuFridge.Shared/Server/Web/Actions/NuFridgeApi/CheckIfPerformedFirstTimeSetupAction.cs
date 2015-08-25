using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Extensions;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class CheckIfPerformedFirstTimeSetupAction : IAction
    {
        private readonly IUserService _userService;

        public CheckIfPerformedFirstTimeSetupAction(IUserService userService)
        {
            _userService = userService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            int userCount = _userService.GetCount();

            if (module.Request.IsLocal() && userCount == 0)
            {
                return module.Negotiate.WithModel(false);
            }

            return module.Negotiate.WithModel(true);
        }
    }
}