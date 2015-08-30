using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class UpdateAccountAction : IAction
    {
        private readonly IUserService _userService;

        public UpdateAccountAction(IUserService userService)
        {
            _userService = userService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string username = parameters.username;

            module.RequiresAuthentication();

            User user = module.Bind<User>();

            if (!string.IsNullOrWhiteSpace(username))
            {
                if (module.Context.CurrentUser.HasAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanUpdateUsers }))
                {
                    if (user.Username != username)
                    {
                        return new Response {StatusCode = HttpStatusCode.BadRequest};
                    }

                    return UpdateUser(user);
                }
            }
            else
            {
                return UpdateUser(user);
            }

            return new Response { StatusCode = HttpStatusCode.Unauthorized };
        }

        private dynamic UpdateUser(User user)
        {
            var existingUser = _userService.Find(user.Username, true);

            existingUser.DisplayName = user.DisplayName;
            existingUser.EmailAddress = user.EmailAddress;

            _userService.Update(existingUser);

            return new Response {StatusCode = HttpStatusCode.OK};
        }
    }
}