using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using Nancy;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.OData;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class GetODataPackagesAction : IAction
    {
        private readonly IFrameworkNamesRepository _frameworkNamesRepository;
        protected readonly IStore Store;
        private readonly IWebPortalConfiguration _portalConfig;

        public GetODataPackagesAction(IFrameworkNamesRepository frameworkNamesRepository, IStore store, IWebPortalConfiguration portalConfig)
        {
            _frameworkNamesRepository = frameworkNamesRepository;
            Store = store;
            _portalConfig = portalConfig;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            var feed = GetFeedModel(feedName);

            if (feed == null)
            {
                var response = module.Response.AsText("Feed does not exist.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            IDictionary<string, object> queryDictionary = module.Request.Query;
            string selectValue = GetAndRemoveSelectParamFromQuery(queryDictionary) ?? String.Empty;

            AddAdditionalQueryParams(queryDictionary);

            NuGetODataModelBuilderQueryable builder = new NuGetODataModelBuilderQueryable();
            builder.Build();

            var url = module.Request.Url.ToString();

            HttpMethod method = new HttpMethod(module.Request.Method);
            var request = new HttpRequestMessage(method, url);
            ListExtensions.AddRange(request.Properties, queryDictionary);

            var context = new ODataQueryContext(builder.Model, typeof(IInternalPackage));

            using (var dbContext = new DatabaseContext(feed.Id, Store))
            {
                bool enableStableOrdering;
                var ds = CreateQuery(dbContext, queryDictionary, feed, out enableStableOrdering);

                ds = ExecuteQuery(context, request, ds, enableStableOrdering);

                return ProcessResponse(module, request, feed, ds, selectValue);
            }
        }

        protected virtual dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<IInternalPackage> ds, string selectFields)
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

        private static IQueryable<IInternalPackage> ExecuteQuery(ODataQueryContext context, HttpRequestMessage request, IQueryable<IInternalPackage> ds, bool enableStableOrdering)
        {
            ODataQueryOptions options = new ODataQueryOptions(context, request);

            var settings = new ODataQuerySettings
            {
                PageSize = options.Top?.Value ?? 15,
                EnsureStableOrdering = enableStableOrdering
            };


            ds = options.ApplyTo(ds, settings) as IQueryable<IInternalPackage>;
            return ds;
        }

        private string RemoveQuotesFromQueryValue(string value)
        {
            int trimStart = value.StartsWith("'") ? 1 : 0;
            int trimEnd = value.EndsWith("'") ? (value.Length - trimStart - 1) : (value.Length - trimStart);
            return value.Substring(trimStart, trimEnd);
        }

        protected virtual IQueryable<IInternalPackage> CreateQuery(DatabaseContext dbContext, IDictionary<string, object> queryDictionary, IFeed feed, out bool enableStableOrdering)
        {
            enableStableOrdering = true;

            IQueryable<IInternalPackage> ds = dbContext.Packages.AsNoTracking().AsQueryable();

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

                    string[] searchFrameworks = _frameworkNamesRepository.Get()
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

        private IFeed GetFeedModel(string feedName)
        {
            IFeed feed;
            using (ITransaction transaction = Store.BeginTransaction())
            {
                feed =
                    transaction.Query<IFeed>()
                        .Where("Name = @feedName")
                        .Parameter("feedName", feedName)
                        .First();
            }
            return feed;
        }
    }
}