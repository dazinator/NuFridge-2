using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class TabCompletionPackageIdsAction : PackagesBase, IAction
    {
        private readonly IFeedService _feedService;
        private const int PackagesToReturn = 30;

        public TabCompletionPackageIdsAction(IStore store, IFeedService feedService)
            : base(store)
        {
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            IFeed feed = _feedService.Find(feedName, false);

            if (feed == null)
            {
                var response = module.Response.AsText($"Feed does not exist called {feedName}.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            IDictionary<string, object> queryDictionary = module.Request.Query;

            string partialId = string.Empty;
            bool includePrerelease = false;

            if (queryDictionary.ContainsKey("partialId"))
            {
                partialId = queryDictionary["partialId"].ToString();
            }

            if (queryDictionary.ContainsKey("includePrerelease"))
            {
                includePrerelease = Boolean.Parse(queryDictionary["includePrerelease"].ToString());
            }

            using (var dbContext = new DatabaseContext())
            {
                var query = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id);

                if (includePrerelease)
                {
                    query = query.Where(pk => pk.IsLatestVersion || pk.IsAbsoluteLatestVersion);
                }
                else
                {
                    query = query.Where(pk => pk.IsLatestVersion);
                }

                query = query.Where(pk => pk.Listed);

                if (!string.IsNullOrWhiteSpace(partialId))
                {
                    query = query.Where(pk => pk.Id.Contains(partialId));
                }

                return module.Response.AsJson(query.Select(pk => pk.Id).Distinct().Take(PackagesToReturn));
            }
        }
    }
}