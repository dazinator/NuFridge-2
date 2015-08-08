using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedDownloadCountAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;
        private readonly IStatisticService _statisticService;

        public GetFeedDownloadCountAction(IFeedService feedService, IPackageService packageService, IStatisticService statisticService)
        {
            _feedService = feedService;
            _packageService = packageService;
            _statisticService = statisticService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var model = new FeedDownloadCountStatistic(_feedService, _packageService, _statisticService).GetModel();

            return model;
        }
    }
}