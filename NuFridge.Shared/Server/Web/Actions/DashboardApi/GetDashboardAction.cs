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

        public GetDashboardAction(IFeedService feedService)
        {
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            using (var dbContext = new DatabaseContext())
            {
                var feedsCount = _feedService.GetCount();
                var usersCount = dbContext.Users.AsNoTracking().Count();

                return new
                {
                    feedCount = feedsCount,
                    userCount = usersCount
                };
            }
        }
    }
}