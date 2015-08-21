using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Web.Responses;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class FeedSearchAction : IAction
    {
        private readonly IFeedService _feedService;

        public FeedSearchAction(IFeedService feedService)
        {
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            FeedSearchResponse response = new FeedSearchResponse();

            string name = module.Request.Query.name;

            IEnumerable<Feed> feeds = _feedService.Search(name).OrderBy(f => f.Name).Take(10);

            var category = new FeedSearchResponse.Category("Default");
            response.Results.Add(category);

            string rootUrl = module.Request.Url.SiteBase + "/#feeds/view/{0}";

            foreach (var feed in feeds)
            {
                category.Feeds.Add(new FeedSearchResponse.Category.FeedResult(feed.Name, string.Format(rootUrl, feed.Id)));
            }

            return response;
        }
    }
}