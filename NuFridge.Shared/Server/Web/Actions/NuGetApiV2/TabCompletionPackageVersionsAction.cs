using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class TabCompletionPackageVersionsAction : PackagesBase, IAction
    {
        private const int PackagesToReturn = 30;

        public TabCompletionPackageVersionsAction(IStore store) : base(store)
        {
            
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            IFeed feed;

            using (var dbContext = new DatabaseContext())
            {
                feed =
                    dbContext.Feeds.AsNoTracking()
                        .FirstOrDefault(f => f.Name.Equals(feedName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (feed == null)
            {
                var response = module.Response.AsText("Feed does not exist.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            IDictionary<string, object> queryDictionary = module.Request.Query;

            string packageId = parameters.packageId;
            bool includePrerelease = false;

            if (queryDictionary.ContainsKey("includePrerelease"))
            {
                includePrerelease = Boolean.Parse(queryDictionary["includePrerelease"].ToString());
            }

            using (var dbContext = new DatabaseContext())
            {
                var query = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id);

                if (!includePrerelease)
                {
                    query = query.Where(pk => !pk.IsPrerelease);
                }


                query = query.Where(pk => pk.Id.Contains(packageId));

                return module.Response.AsJson(query.Take(PackagesToReturn));
            }
        }
    }
}