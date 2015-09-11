using System.Linq;
using System.Net;
using Nancy;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class CheckIfPerformedFirstTimeSetupAction : IAction
    {
        private readonly IUserService _userService;

        public CheckIfPerformedFirstTimeSetupAction(IUserService userService)
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
            int userCount = _userService.GetCount();

            if (IsLocalhost(module.Request.UserHostAddress) && userCount == 0)
            {
                return module.Negotiate.WithModel(false);
            }

            return module.Negotiate.WithModel(true);
        }
    }
}