using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedDownloadCountAction : IAction
    {
        public GetFeedDownloadCountAction()
        {

        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var model = new FeedDownloadCountStatistic().GetModel();

            return model;

        }
    }
}