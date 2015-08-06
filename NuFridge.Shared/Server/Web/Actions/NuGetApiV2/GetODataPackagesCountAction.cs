using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class GetODataPackagesCountAction : GetODataPackagesAction
    {

        public GetODataPackagesCountAction(IFrameworkNamesManager frameworkNamesManager, IStore store, IWebPortalConfiguration webConfig, IFeedService feedService)
            : base(frameworkNamesManager, store, webConfig, feedService)
        {

        }

        protected override void AddAdditionalQueryParams(IDictionary<string, object> queryDictionary)
        {
            if (!queryDictionary.ContainsKey("%inlinecount"))
            {
                queryDictionary.Add("$inlinecount", "allpages");
            }
        }

        protected override dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<InternalPackage> ds, string selectValue)
        {
            long? total = request.ODataProperties().TotalCount;

            return new TextResponse(HttpStatusCode.OK, total.ToString());
        }
    }
}