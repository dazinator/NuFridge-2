using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedPackageCountAction : IAction
    {
        private readonly IStore _store;

        public GetFeedPackageCountAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

          
                var model = new FeedPackageCountStatistic(_store).GetModel();

                return model;
            
        }
    }
}