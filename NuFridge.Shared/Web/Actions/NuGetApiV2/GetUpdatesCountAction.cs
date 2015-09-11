using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Web.Actions.NuGetApiV2
{
    public class GetUpdatesCountAction : GetUpdatesAction
    {
        public GetUpdatesCountAction(IWebPortalConfiguration webConfig, IFeedService feedService, IPackageService packageService)
            : base(webConfig, feedService, packageService)
        {

        }

        protected override void AddAdditionalQueryParams(IDictionary<string, object> queryDictionary)
        {
            if (!queryDictionary.ContainsKey("%inlinecount"))
            {
                queryDictionary.Add("$inlinecount", "allpages");
            }
        }

        protected override dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<IInternalPackage> ds, string selectValue)
        {
            long? total = request.ODataProperties().TotalCount;

            return new TextResponse(HttpStatusCode.OK, total.ToString());
        }
    }
}