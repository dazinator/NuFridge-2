using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.NuGet;
using NuFridge.Shared.Security;
using NuFridge.Shared.Web.OData;
using NuGet;

namespace NuFridge.Shared.Web.Actions.NuGetApiV2
{
    public class GetODataPackagesAction : IAction
    {
        private readonly IFrameworkNamesManager _frameworkNamesManager;
        protected readonly IStore Store;
        private readonly IWebPortalConfiguration _portalConfig;
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public GetODataPackagesAction(IFrameworkNamesManager frameworkNamesManager, IStore store, IWebPortalConfiguration portalConfig, IFeedService feedService, IPackageService packageService)
        {
            _frameworkNamesManager = frameworkNamesManager;
            Store = store;
            _portalConfig = portalConfig;
            _feedService = feedService;
            _packageService = packageService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            var feed = _feedService.Find(feedName, false);

            if (feed == null)
            {
                var response = module.Response.AsText($"Feed does not exist called {feedName}.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (module.Context.CurrentUser != null && module.Context.CurrentUser.IsAuthenticated())
            {
                module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanViewPackages });
            }

            IDictionary<string, object> queryDictionary = module.Request.Query;
            string selectValue = GetAndRemoveSelectParamFromQuery(queryDictionary) ?? String.Empty;

            AddAdditionalQueryParams(queryDictionary);

            NuGetODataModelBuilderQueryable builder = new NuGetODataModelBuilderQueryable();
            builder.Build();

            var url = module.Request.Url.SiteBase + module.Request.Url.Path;
            url = ProcessQueryAndRegenerateUrl(queryDictionary, url);

            HttpMethod method = new HttpMethod(module.Request.Method);
            var request = new HttpRequestMessage(method, url);
            ListExtensions.AddRange(request.Properties, queryDictionary);

            var context = new ODataQueryContext(builder.Model, typeof (InternalPackage));

            bool enableStableOrdering;
            var ds = CreateQuery(queryDictionary, feed, out enableStableOrdering);

            ds = ExecuteQuery(context, request, ds.Cast<InternalPackage>(), enableStableOrdering);

            return ProcessResponse(module, request, feed, ds, selectValue);
        }

        private string ProcessQueryAndRegenerateUrl(IDictionary<string, object> queryDictionary, string url)
        {
            var query = string.Empty;

            foreach (KeyValuePair<string, object> qd in queryDictionary)
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    query = "?";
                }
                query += qd.Key + "=" + qd.Value + "&";
            }

            if (query.EndsWith("&"))
            {
                query = query.Substring(0, query.Length - 1);
            }

            url += query;

            return url;
        }

        protected virtual dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<IInternalPackage> ds, string selectFields)
        {
            long? total = request.ODataProperties().TotalCount;

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var baseAddress = $"{_portalConfig.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feed.Name}/api/v2/";

            var enumerable = module.Request.Headers.Accept;
            var ranges = enumerable.OrderByDescending(o => o.Item2).Select(o => new MediaRange(o.Item1)).ToList();

            bool isXmlResponse = false;

            foreach (var mediaRange in ranges)
            {
                if (mediaRange.Matches("application/xml") || mediaRange.Matches("application/atom+xml;type=feed") || mediaRange.Matches("application/atom+xml"))
                {
                    isXmlResponse = true;
                }
            }

            var stream = ODataPackages.CreatePackagesStream(baseAddress, baseAddress, ds, total.HasValue ? int.Parse(total.Value.ToString()) : 0, selectFields, isXmlResponse);

            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();

            return new Response
            {
                ContentType = isXmlResponse ? "application/atom+xml; charset=utf-8" : "application/json;odata=verbose;charset=utf-8",
                Contents = contentStream =>
                {
                    var byteData = Encoding.UTF8.GetBytes(text);
                    contentStream.Write(byteData, 0, byteData.Length);
                }
            };
        }

        private static IQueryable<IInternalPackage> ExecuteQuery(ODataQueryContext context, HttpRequestMessage request, IQueryable<InternalPackage> ds, bool enableStableOrdering)
        {
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            var settings = new ODataQuerySettings
            {
                PageSize = options.Top?.Value ?? 15,
                EnsureStableOrdering = enableStableOrdering
            };

            ds = options.ApplyTo(ds, settings) as IQueryable<InternalPackage>;
            return ds;
        }

        private string RemoveQuotesFromQueryValue(string value)
        {
            int trimStart = value.StartsWith("'") ? 1 : 0;
            int trimEnd = value.EndsWith("'") ? (value.Length - trimStart - 1) : (value.Length - trimStart);
            return value.Substring(trimStart, trimEnd);
        }



        protected virtual IQueryable<IInternalPackage> CreateQuery(IDictionary<string, object> queryDictionary, IFeed feed, out bool enableStableOrdering)
        {
            enableStableOrdering = true;

            IQueryable<IInternalPackage> ds = _packageService.GetAllPackagesForFeed(feed.Id).AsQueryable();

            ds = ds.Where(pk => pk.Listed);

            string searchTerm = queryDictionary.ContainsKey("searchTerm")
                ? RemoveQuotesFromQueryValue(queryDictionary["searchTerm"].ToString())
                : string.Empty;

            string targetFramework = queryDictionary.ContainsKey("targetFramework")
                ? RemoveQuotesFromQueryValue(queryDictionary["targetFramework"].ToString())
                : string.Empty;

            string idSearch = queryDictionary.ContainsKey("id")
                ? RemoveQuotesFromQueryValue(queryDictionary["id"].ToString())
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(idSearch))
            {
                ds = ds.Where(pk => pk.Id == idSearch);
                ds = ds.OrderByDescending(pk => pk.VersionMajor)
                    .ThenByDescending(pk => pk.VersionMinor)
                    .ThenByDescending(pk => pk.VersionBuild)
                    .ThenByDescending(pk => pk.VersionRevision);

                enableStableOrdering = false;
            }

            if (queryDictionary.ContainsKey("includePrerelease"))
            {
                bool includePrerelease = bool.Parse(queryDictionary["includePrerelease"].ToString());


                if (!includePrerelease)
                {
                    ds = ds.Where(pk => !pk.IsPrerelease);
                }
            }

            if (!string.IsNullOrWhiteSpace(targetFramework))
            {
                var targetFrameworkValue = ((targetFramework ?? "")
                    .Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(VersionUtility.ParseFrameworkName).Distinct().Where(v => v != VersionUtility.UnsupportedFrameworkName)
                    .ToList()).FirstOrDefault();

                if (targetFrameworkValue != null)
                {

                    string[] searchFrameworks = _frameworkNamesManager.Get()
                        .Union(new[] { targetFrameworkValue })
                        .Where(candidate => VersionUtility.IsCompatible(targetFrameworkValue, new[] { candidate }))
                        .Select(VersionUtility.GetShortFrameworkName)
                        .Select(s => "|" + s.ToLowerInvariant() + "|")
                        .Distinct()
                        .ToArray();

                    ds = ds.Where(pkg => searchFrameworks.Any(sf => ("|" + pkg.SupportedFrameworks + "|").Contains(sf)));
                }
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var splitTerms =
                    searchTerm.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.ToLower())
                        .Distinct().ToList();

                if (splitTerms.Any())
                {
                    ds = ds.Where(pk => splitTerms.Any(sc => pk.Id.ToLower().Contains(sc.ToLower())));
                }
            }
            return ds;
        }

        protected virtual void AddAdditionalQueryParams(IDictionary<string, object> queryDictionary)
        {
            
        }

        private static string GetAndRemoveSelectParamFromQuery(IDictionary<string, object> queryDictionary)
        {
            if (queryDictionary.ContainsKey("$select"))
            {
                DynamicDictionaryValue select = (DynamicDictionaryValue)queryDictionary["$select"];
                queryDictionary.Remove("$select");
                var value = (string)select.Value;
                return value;
            }

            return null;
        }
    }
}