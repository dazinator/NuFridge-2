using System.Collections.Generic;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Security;
using NuFridge.Shared.Server.Statistics;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
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
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanViewDashboard});

            var model = new FeedDownloadCountStatistic(_feedService, _packageService, _statisticService).GetModel();

            return model;
        }
    }
}