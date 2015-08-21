using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class GetFeedsAction : IAction
    {
        private readonly IFeedService _feedService;

        public GetFeedsAction(IFeedService feedService)
        {
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int page = int.Parse(module.Request.Query["page"]);
            int pageSize = int.Parse(module.Request.Query["pageSize"]);

            int totalResults = _feedService.GetCount();
            List<Feed> feeds = _feedService.GetAllPaged(page, pageSize, false).ToList();

            var totalPages = (int)Math.Ceiling((double)totalResults / pageSize);

            return new
            {
                TotalCount = totalResults,
                TotalPages = totalPages,
                Results = feeds
            };
        }
    }
}