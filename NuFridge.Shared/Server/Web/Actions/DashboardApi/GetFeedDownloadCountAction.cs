using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedDownloadCountAction : IAction
    {
        private readonly IStore _store;

        public GetFeedDownloadCountAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            
            
                var model = new FeedDownloadCountStatistic().GetModel();

                return model;
            
        }
    }
}