using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetDashboardAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly IUserService _userService;

        public GetDashboardAction(IFeedService feedService, IUserService userService)
        {
            _feedService = feedService;
            _userService = userService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var feedsCount = _feedService.GetCount();
            var usersCount = _userService.GetCount();

            return new
            {
                feedCount = feedsCount,
                userCount = usersCount
            };
        }
    }
}