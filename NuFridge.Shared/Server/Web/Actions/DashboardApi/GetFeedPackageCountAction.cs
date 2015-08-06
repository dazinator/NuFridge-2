using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedPackageCountAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public GetFeedPackageCountAction(IFeedService feedService, IPackageService packageService)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var model = new FeedPackageCountStatistic(_feedService, _packageService).GetModel();

            return model;
        }
    }
}