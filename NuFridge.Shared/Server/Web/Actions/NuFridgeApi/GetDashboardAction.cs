using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class GetDashboardAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly IUserService _userService;
        private readonly IPackageService _packageService;
        private readonly IPackageDownloadService _packageDownloadService;

        public GetDashboardAction(IFeedService feedService, IUserService userService, IPackageService packageService, IPackageDownloadService packageDownloadService)
        {
            _feedService = feedService;
            _userService = userService;
            _packageService = packageService;
            _packageDownloadService = packageDownloadService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var feedsCount = _feedService.GetCount();
            var usersCount = _userService.GetCount();
            var packagesCount = _packageService.GetCount();
            var downloadCount = _packageDownloadService.GetCount();

            return module.Negotiate.WithModel(new
            {
                FeedCount = feedsCount,
                UserCount = usersCount,
                DownloadCount = downloadCount,
                PackageCount = packagesCount
            });
        }
    }
}