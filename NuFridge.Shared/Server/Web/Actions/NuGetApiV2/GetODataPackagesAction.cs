using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Web.Http.OData;
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
        protected readonly IInternalPackageRepositoryFactory PackageRepositoryFactory;
        protected readonly IStore Store;
        private readonly IWebPortalConfiguration _portalConfig;

        public GetODataPackagesAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IWebPortalConfiguration portalConfig)
        {
            PackageRepositoryFactory = packageRepositoryFactory;
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

            var url = module.Request.Url.SiteBase + module.Request.Url.Path;
            url = ProcessQueryAndRegenerateUrl(queryDictionary, url);

            HttpMethod method = new HttpMethod(module.Request.Method);
            var request = new HttpRequestMessage(method, url);
            ListExtensions.AddRange(request.Properties, queryDictionary);

            var context = new ODataQueryContext(builder.Model, typeof(IInternalPackage));

            using (var dbContext = new DatabaseContext(Store))
            {
                var ds = CreateQuery(dbContext, queryDictionary, feed);

                ds = ExecuteQuery(context, request, ds);

                return ProcessResponse(module, request, feed, ds, selectValue);
            }
        }

        protected virtual dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<IInternalPackage> ds, string selectFields)
        {
            long? total = request.GetInlineCount();

            var packageRepository = PackageRepositoryFactory.Create(feed.Id);

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var baseAddress = string.Format("{0}{1}feeds/{2}/api/v2/", _portalConfig.ListenPrefixes, endsWithSlash ? "" : "/", feed.Name);

            var stream = ODataPackages.CreatePackagesStream(baseAddress, packageRepository, baseAddress,
                ds, feed.Id, total.HasValue ? int.Parse(total.Value.ToString()) : 0, selectFields);

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

        private static IQueryable<IInternalPackage> ExecuteQuery(ODataQueryContext context, HttpRequestMessage request, IQueryable<IInternalPackage> ds)
        {
            ODataQueryOptions options = new ODataQueryOptions(context, request);
            
            var settings = new ODataQuerySettings
            {
                PageSize = options.Top != null ? options.Top.Value : 15
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

        protected virtual IQueryable<IInternalPackage> CreateQuery(DatabaseContext dbContext, IDictionary<string, object> queryDictionary, IFeed feed)
        {
            IQueryable<IInternalPackage> ds = dbContext.Packages.AsNoTracking().AsQueryable();

            ds = ds.Where(pk => pk.FeedId == feed.Id);
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
                ds = ds.Where(pk => pk.PackageId == idSearch);
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

                    string[] searchFrameworks = PackageRepositoryFactory.GetFrameworkNames()
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
                    ds = ds.Where(pk => splitTerms.Any(sc => pk.PackageId.ToLower().Contains(sc.ToLower())));
                }
            }
            return ds;
        }

        private string ProcessQueryAndRegenerateUrl(IDictionary<string, object> queryDictionary, string url)
        {
            var query = string.Empty;

            var updatesToApply = new List<KeyValuePair<string, object>>();

            //Start - this section could be better
            foreach (KeyValuePair<string, object> qd in queryDictionary)
            {
                string updatedValue;
                if (GetReplacedODataQueryValue(qd, out updatedValue))
                {
                    updatesToApply.Add(new KeyValuePair<string, object>(qd.Key, updatedValue));
                }
            }

            foreach (var updateToApply in updatesToApply)
            {
                queryDictionary[updateToApply.Key] = updateToApply.Value;
            }

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
            //End - this section could be better

            url += query;
            return url;
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
                return (string)select.Value;
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

        //This method could be removed if it used an interface to mask the fact Id is my PK and not the PackageId
        private bool GetReplacedODataQueryValue(KeyValuePair<string, object> kvp, out string updatedString)
        {
            bool updated = false;
            updatedString = null;
            var value = kvp.Value;
            if (value != null)
            {
                var str = value.ToString();
                var split = str.Split(',');
                if (split.Any())
                {
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (split[i].ToLower() == "id")
                        {
                            updated = true;
                            split[i] = "PackageId";
                        }
                    }
                }
                updatedString = string.Join(",", split);
            }
            return updated;
        }
    }
}