using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Security;
using NuFridge.Shared.Server.Web.Responses;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class FeedSearchAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly IFeedGroupService _feedGroupService;

        public FeedSearchAction(IFeedService feedService, IFeedGroupService feedGroupService)
        {
            _feedService = feedService;
            _feedGroupService = feedGroupService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanViewFeeds });

            FeedSearchResponse response = new FeedSearchResponse();

            string name = module.Request.Query.name;

            IEnumerable<Feed> feeds = _feedService.Search(name).OrderBy(f => f.Name).Take(10);

            Dictionary<int, FeedSearchResponse.Category> categories = new Dictionary<int, FeedSearchResponse.Category>();

            string rootUrl = module.Request.Url.SiteBase + "/#feeds/view/{0}";

            foreach (var feed in feeds)
            {
                if (categories.ContainsKey(feed.GroupId))
                {
                    categories[feed.GroupId].Feeds.Add(new FeedSearchResponse.Category.FeedResult(feed.Name,
                        string.Format(rootUrl, feed.Id)));
                }
                else
                {
                    var feedGroup = _feedGroupService.Find(feed.GroupId);
                    var category = new FeedSearchResponse.Category(feedGroup.Name);
                    category.Feeds.Add(new FeedSearchResponse.Category.FeedResult(feed.Name,
                        string.Format(rootUrl, feed.Id)));
                    categories.Add(feed.GroupId, category);
                }
            }

            response.Results.AddRange(categories.Values);

            return response;
        }
    }
}