using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetDashboardAction : IAction
    {
        private readonly IStore _store;

        public GetDashboardAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            using (var dbContext = new DatabaseContext())
            {
                var feedsCount = dbContext.Feeds.AsNoTracking().Count();
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