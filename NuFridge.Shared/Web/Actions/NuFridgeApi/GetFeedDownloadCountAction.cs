using System;
using System.Collections.Generic;
using Nancy;
using Nancy.LightningCache.Extensions;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Reporting;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
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
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanViewDashboard});

            var model = new FeedDownloadCountReport(_feedService, _packageService).GetModel();

            return
                module.Negotiate.WithModel(model)
                    .WithStatusCode(HttpStatusCode.OK)
                    .AsCacheable(DateTime.Now.AddMinutes(30));
        }
    }
}