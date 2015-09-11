using System.Linq;
using System.Net;
using Nancy;
using Nancy.ModelBinding;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class SetupAction : IAction
    {
        private readonly IUserService _userService;

        public SetupAction(IUserService userService)
        {
            _userService = userService;
        }

        public static bool IsLocalhost(string hostNameOrAddress)
        {
            if (string.IsNullOrEmpty(hostNameOrAddress))
                return false;

            try
            {
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(hostNameOrAddress);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                // test if any host IP is a loopback IP or is equal to any local IP
                return hostIPs.Any(hostIP => IPAddress.IsLoopback(hostIP) || localIPs.Contains(hostIP));
            }
            catch
            {
                return false;
            }
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            RequestMessage request = module.Bind<RequestMessage>();

            if (!IsLocalhost(module.Request.UserHostAddress))
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