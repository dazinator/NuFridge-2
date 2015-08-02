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

            string packageId = parameters.packageId;
            bool includePrerelease = false;

            if (queryDictionary.ContainsKey("includePrerelease"))
            {
                includePrerelease = Boolean.Parse(queryDictionary["includePrerelease"].ToString());
            }

            using (var transaction = Store.BeginTransaction())
            {
                var query = transaction.Query<IInternalPackage>()
                    .Where(feed.Id);

                if (!includePrerelease)
                {
                    query = query.Where("IsPrerelease = 0");
                }

                query = query.Where("PackageId LIKE @packageId")
                    .Parameter("packageId", packageId);

                packages = query.ToList(0, PackagesToReturn);
            }

            return module.Response.AsJson(packages.Select(pk => pk.Version));
        }
    }
}