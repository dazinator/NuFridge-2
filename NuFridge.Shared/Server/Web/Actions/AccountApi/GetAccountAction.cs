using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.AccountApi
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
                //TODO once roles are in place
                if (module.Context.CurrentUser.UserName.ToLower() == "administrator")
                {
                    return GetUser(username);
                }
            }
            else
            {
                return GetUser(module.Context.CurrentUser.UserName);
            }

            return new Response {StatusCode = HttpStatusCode.Unauthorized};
        }

        private User GetUser(string username)
        {
            var user = _userService.Find(username, false);

            return user;
        }
    }
}