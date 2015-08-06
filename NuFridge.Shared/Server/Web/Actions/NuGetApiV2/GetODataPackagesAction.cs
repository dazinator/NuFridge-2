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
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.OData;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class GetODataPackagesAction : IAction
    {
        private readonly IFrameworkNamesManager _frameworkNamesManager;
        protected readonly IStore Store;
        private readonly IWebPortalConfiguration _portalConfig;
        private readonly IFeedService _feedService;

        public GetODataPackagesAction(IFrameworkNamesManager frameworkNamesManager, IStore store, IWebPortalConfiguration portalConfig, IFeedService feedService)
        {
            _frameworkNamesManager = frameworkNamesManager;
            Store = store;
            _portalConfig = portalConfig;
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            var feed = _feedService.Find(feedName, false);

            if (feed == null)
            {
                var response = module.Response.AsText($"Feed does not exist called {feedName}.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
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

            var context = new ODataQueryContext(builder.Model, typeof(InternalPackage));

            using (var dbContext = new DatabaseContext())
            {
                bool enableStableOrdering;
                var ds = CreateQuery(dbContext, queryDictionary, feed, out enableStableOrdering);

                ds = ExecuteQuery(context, request, ds, enableStableOrdering);

                return ProcessResponse(module, request, feed, ds, selectValue);
            }
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

        protected virtual dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<InternalPackage> ds, string selectFields)
        {
            long? total = request.ODataProperties().TotalCount;

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var baseAddress = $"{_portalConfig.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feed.Name}/api/v2/";

            var stream = ODataPackages.CreatePackagesStream(baseAddress, baseAddress, ds, total.HasValue ? int.Parse(total.Value.ToString()) : 0, selectFields);

            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();

            return new Response
            {
                ContentType = "application/atom+xml; charset=utf-8",
                Contents = contentStream =>
                {
                    var byteData = Encoding.UTF8.GetBytes(text);
                    contentStream.Write(byteData, 0, byteData.Length);
                }
            };
        }

        private static IQueryable<InternalPackage> ExecuteQuery(ODataQueryContext context, HttpRequestMessage request, IQueryable<InternalPackage> ds, bool enableStableOrdering)
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



        protected virtual IQueryable<InternalPackage> CreateQuery(DatabaseContext dbContext, IDictionary<string, object> queryDictionary, IFeed feed, out bool enableStableOrdering)
        {
            enableStableOrdering = true;

            //var query = dbContext.Database.SqlQuery<InternalPackage>("NuFridge.GetAllPackages @feedId", new SqlParameter("feedId", feed.Id));

            IQueryable<InternalPackage> ds = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id);

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