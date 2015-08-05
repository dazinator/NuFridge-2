using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class GetFeedsAction : IAction
    {
        private readonly IStore _store;

        public GetFeedsAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int page = int.Parse(module.Request.Query["page"]);
            int pageSize = int.Parse(module.Request.Query["pageSize"]);
            int totalResults;

            List<Feed> feeds;
            using (var dbContext = new DatabaseContext())
            {
                feeds = dbContext.Feeds.AsNoTracking().OrderBy(f => f.Name).Skip(pageSize*page).Take(pageSize).ToList();
                totalResults = dbContext.Feeds.AsNoTracking().Count();
            }

            feeds.Where(fd => !string.IsNullOrWhiteSpace(fd.ApiKeyHashed)).ToList().ForEach(fd => fd.HasApiKey = true);
            feeds.ForEach(fd => fd.ApiKeyHashed = null); //Temporary until API Key table is used
            feeds.ForEach(fd => fd.ApiKeySalt = null); //Temporary until API Key table is used

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