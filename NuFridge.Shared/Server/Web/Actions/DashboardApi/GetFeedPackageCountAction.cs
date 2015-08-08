using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedPackageCountAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;
        private readonly IStatisticService _statisticService;

        public GetFeedPackageCountAction(IFeedService feedService, IPackageService packageService, IStatisticService statisticService) 
        {
            _feedService = feedService;
            _packageService = packageService;
            _statisticService = statisticService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var model = new FeedPackageCountStatistic(_feedService, _packageService, _statisticService).GetModel();

            return model;
        }
    }
}