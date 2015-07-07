using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Model;
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

            List<IFeed> feeds;
            using (ITransaction transaction = _store.BeginTransaction())
            {
                feeds = transaction.Query<IFeed>().ToList(pageSize * page, pageSize, out totalResults);
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