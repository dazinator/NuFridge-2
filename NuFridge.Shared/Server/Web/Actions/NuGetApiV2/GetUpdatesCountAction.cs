using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class GetUpdatesCountAction : GetUpdatesAction
    {

        public GetUpdatesCountAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IWebPortalConfiguration webConfig)
            : base(packageRepositoryFactory, store, webConfig)
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