using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.OData;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApi
{
    public class GetODataPackagesCountAction : GetODataPackagesAction
    {

        public GetODataPackagesCountAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IWebPortalConfiguration webConfig)
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

        protected override dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<IInternalPackage> ds)
        {
            long? total = request.GetInlineCount();

            return new TextResponse(HttpStatusCode.OK, total.ToString());
        }
    }
}