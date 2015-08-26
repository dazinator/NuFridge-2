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
        private readonly IPackageService _packageService;
        private const int PackagesToReturn = 30;

        public TabCompletionPackageIdsAction(IStore store, IFeedService feedService, IPackageService packageService)
            : base(store)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            IFeed feed = _feedService.Find(feedName, false);

            if (feed == null)
            {
                var response = module.Response.AsText($"Feed does not exist called {feedName}.");
                response.StatusCode = HttpStatusCode.NotFound;
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

            var packages = _packageService.GetLatestPackagesForFeed(feed.Id, includePrerelease, partialId);

            return module.Response.AsJson(packages.Select(pk => pk.Id).Distinct().Take(PackagesToReturn));
        }
    }
}