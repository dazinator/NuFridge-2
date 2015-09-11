using System;
using System.Collections.Generic;
using Nancy;
using Nancy.LightningCache.Extensions;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
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
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanViewDashboard });

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
            }).AsCacheable(DateTime.Now.AddMinutes(1));
        }
    }
}