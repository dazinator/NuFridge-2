using System.Collections.Generic;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class GetAccountAction : IAction
    {
        private readonly IUserService _userService;

        public GetAccountAction(IUserService userService)
        {
            _userService = userService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string username = parameters.username;

            module.RequiresAuthentication();

            if (!string.IsNullOrWhiteSpace(username))
            {
                if (module.Context.CurrentUser.HasAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanViewUsers }))
                {
                    return GetUser(module, username);
                }
            }
            else
            {
                return GetUser(module, module.Context.CurrentUser.UserName);
            }

            return new Response { StatusCode = HttpStatusCode.Unauthorized };
        }

        private User GetUser(INancyModule module, string username)
        {
            var user = _userService.Find(username, false);

            if (username == module.Context.CurrentUser.UserName)
            {
                user.Claims = module.Context.CurrentUser.Claims;
            }

            return user;
        }
    }
}