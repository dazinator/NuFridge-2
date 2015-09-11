using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class UpdateAccountAction : IAction
    {
        private readonly IUserService _userService;
        private readonly ILog _log = LogProvider.For<UpdateAccountAction>();

        public UpdateAccountAction(IUserService userService)
        {
            _userService = userService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            try
            {
                User user = module.Bind<User>();

                if (user.Username == module.Context.CurrentUser.UserName)
                {
                    return UpdateUser(user);
                }


                module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanUpdateUsers });
                return UpdateUser(user);
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return module.Negotiate.WithStatusCode(HttpStatusCode.InternalServerError).WithModel(ex.Message);
            }
        }

        private dynamic UpdateUser(User user)
        {
            var existingUser = _userService.Find(user.Username, true);

            existingUser.DisplayName = user.DisplayName;
            existingUser.EmailAddress = user.EmailAddress;

            _userService.Update(existingUser);

            return new Response { StatusCode = HttpStatusCode.OK };
        }
    }
}