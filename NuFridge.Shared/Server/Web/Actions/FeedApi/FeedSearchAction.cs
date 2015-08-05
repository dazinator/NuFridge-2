using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.Responses;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class FeedSearchAction : IAction
    {
        private readonly IStore _store;

        public FeedSearchAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            FeedSearchResponse response = new FeedSearchResponse();
            using (var dbContext = new DatabaseContext())
            {
                string name = module.Request.Query.name;

                var feeds = dbContext.Feeds.AsNoTracking().Where(fc => fc.Name.Contains(name)).OrderBy(fc => fc.Name).Take(10);
                var category = new FeedSearchResponse.Category("Default");
                response.Results.Add(category);

                string rootUrl = module.Request.Url.SiteBase + "/#feeds/view/{0}";

                foreach (var feed in feeds)
                {
                    category.Feeds.Add(new FeedSearchResponse.Category.FeedResult(feed.Name, string.Format(rootUrl, feed.Id)));
                }
            }

            return response;
        }
    }
}