using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class SetupAction : IAction
    {
        private readonly IUserService _userService;

        public SetupAction(IUserService userService)
        {
            _userService = userService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            RequestMessage request = module.Bind<RequestMessage>();

            if (!module.Request.IsLocal())
            {
                return module.Negotiate.WithStatusCode(HttpStatusCode.Forbidden);
            }

            int userCount = _userService.GetCount();

            if (userCount > 0)
            {
                return module.Negotiate.WithStatusCode(HttpStatusCode.Forbidden);
            }

            _userService.CreateSystemUserIfNotExist();

            User user = new User(request.Username)
            {
                EmailAddress = request.EmailAddress,
                Password = request.Password,
                DisplayName = request.Username,
                IsActive = true,
                IsService = false
            };

            _userService.Insert(user);

            return module.Negotiate.WithStatusCode(HttpStatusCode.Created);
        }

        public class RequestMessage
        {
            public string Username { get; set; }
            public string EmailAddress { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}