using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class TabCompletionPackageIdsAction : PackagesBase, IAction
    {
        private const int PackagesToReturn = 30;

        public TabCompletionPackageIdsAction(IStore store)
            : base(store)
        {
            
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            IFeed feed;

            using (var transaction = Store.BeginTransaction())
            {
                feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
            }

            if (feed == null)
            {
                var response = module.Response.AsText("Feed does not exist.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            List<IInternalPackage> packages;

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

            using (var transaction = Store.BeginTransaction())
            {
                var query = transaction.Query<IInternalPackage>()
                    .Where("FeedId = @feedId").Parameter("feedId", feed.Id);

                string latestVersionQuery = "IsLatestVersion = 1";

                if (includePrerelease)
                {
                    latestVersionQuery += " OR IsAbsoluteLatestVersion = 1";
                }

                query = query.Where(latestVersionQuery);
                
                if (!string.IsNullOrWhiteSpace(partialId))
                {
                    query = query.Where("PackageId like @partialId")
                    .Parameter("partialId", partialId.Replace("%", "[%]") + "%");
                }

                query = query.Distinct("PackageId");

                packages = query.ToList(0, PackagesToReturn);
            }

            return module.Response.AsJson(packages.Select(pk => pk.Id));
        }
    }
}