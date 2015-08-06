using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedDownloadCountAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public GetFeedDownloadCountAction(IFeedService feedService, IPackageService packageService)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var model = new FeedDownloadCountStatistic(_feedService, _packageService).GetModel();

            return model;
        }
    }
}