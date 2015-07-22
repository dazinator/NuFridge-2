using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.OData;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class GetUpdatesAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;
        private readonly IWebPortalConfiguration _portalConfig;

        public GetUpdatesAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IWebPortalConfiguration portalConfig)

        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
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
            request.Properties.AddRange(queryDictionary);

            var context = new ODataQueryContext(builder.Model, typeof(IInternalPackage));

            using (var dbContext = new DatabaseContext(_store))
            {
                var ds = CreateQuery(dbContext, queryDictionary, feed);

                ds = ExecuteQuery(context, request, ds);

                return ProcessResponse(module, request, feed, ds, selectValue);
            }
        }

        protected virtual void AddAdditionalQueryParams(IDictionary<string, object> queryDictionary)
        {

        }

        private static string GetAndRemoveSelectParamFromQuery(IDictionary<string, object> queryDictionary)
        {
            if (queryDictionary.ContainsKey("$select"))
            {
                var select = queryDictionary["$select"];
                queryDictionary.Remove("$select");
                return (string)select;
            }

            return null;
        }

        protected virtual dynamic ProcessResponse(INancyModule module, HttpRequestMessage request, IFeed feed, IQueryable<IInternalPackage> ds, string selectValue)
        {
            long? total = request.ODataProperties().TotalCount;

            var packageRepository = _packageRepositoryFactory.Create(feed.Id);

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var baseAddress = $"{_portalConfig.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feed.Name}/api/v2/";

            var stream = ODataPackages.CreatePackagesStream(baseAddress, packageRepository, baseAddress,
                ds, feed.Id, total.HasValue ? int.Parse(total.Value.ToString()) : 0, selectValue);

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
                PageSize = options.Top?.Value ?? 15
            };


            ds = options.ApplyTo(ds, settings) as IQueryable<IInternalPackage>;
            return ds;
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
                        else if (split[i].ToLower().Contains("(id)"))
                        {
                            updated = true;
                            split[i] = split[i].Replace("(id)", "(PackageId)").Replace("(Id)", "(PackageId)").Replace("(ID)", "(PackageId)");
                        }
                    }
                }
                updatedString = string.Join(",", split);
            }
            return updated;
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

        private IFeed GetFeedModel(string feedName)
        {
            IFeed feed;
            using (ITransaction transaction = _store.BeginTransaction())
            {
                feed =
                    transaction.Query<IFeed>()
                        .Where("Name = @feedName")
                        .Parameter("feedName", feedName)
                        .First();
            }
            return feed;
        }

        private string RemoveQuotesFromQueryValue(string value)
        {
            int trimStart = value.StartsWith("'") ? 1 : 0;
            int trimEnd = value.EndsWith("'") ? (value.Length - trimStart -1) : (value.Length - trimStart);
            return value.Substring(trimStart, trimEnd);
        }

        protected IQueryable<IInternalPackage> CreateQuery(DatabaseContext dbContext,
            IDictionary<string, object> queryDictionary, IFeed feed)
        {
            IQueryable<IInternalPackage> ds = dbContext.Packages.AsNoTracking().AsQueryable();

            ds = ds.Where(pk => pk.FeedId == feed.Id);
            ds = ds.Where(pk => pk.Listed);

            string packageIds = queryDictionary.ContainsKey("packageIds")
                ? RemoveQuotesFromQueryValue(queryDictionary["packageIds"].ToString())
                : string.Empty;

            string versions = queryDictionary.ContainsKey("versions")
                ? RemoveQuotesFromQueryValue(queryDictionary["versions"].ToString())
                : string.Empty;

            


            bool includeAllVersions = queryDictionary.ContainsKey("includeAllVersions") && bool.Parse(queryDictionary["includeAllVersions"].ToString());

            string targetFrameworks = queryDictionary.ContainsKey("targetFrameworks")
                ? RemoveQuotesFromQueryValue(queryDictionary["targetFrameworks"].ToString())
                : string.Empty;

            string versionConstraints = queryDictionary.ContainsKey("versionConstraints")
                ? RemoveQuotesFromQueryValue(queryDictionary["versionConstraints"].ToString())
                : string.Empty;


            if (string.IsNullOrEmpty(packageIds) || string.IsNullOrEmpty(versions))
            {
                //TODO
            }

            var idValues = packageIds.Trim().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var versionValues = versions.Trim().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var versionConstraintValues = string.IsNullOrEmpty(versionConstraints)
                                            ? new string[idValues.Length]
                                            : versionConstraints.Trim().Split('|');

            if ((idValues.Length == 0) || (idValues.Length != versionValues.Length) || (idValues.Length != versionConstraintValues.Length))
            {
               //TODO
            }

            var packages = idValues
                .Zip(
                    versionValues.Select(v => new SemanticVersion(v)),
                    (id, version) => new PackageVersion { Id = id, Version = version })
                .ToList();

            var targetFrameworkValues = ((targetFrameworks ?? "")
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(VersionUtility.ParseFrameworkName)
                .ToList());

            var versionSpecs = versionConstraintValues
                .Select((v, iv) => CreateVersionSpec(v, packages[iv].Version))
                .ToList();


            if (queryDictionary.ContainsKey("includePrerelease"))
            {
                bool includePrerelease = bool.Parse(queryDictionary["includePrerelease"].ToString());

                if (!includePrerelease)
                {
                    ds = ds.Where(pk => !pk.IsPrerelease);
                }
            }

            List<IInternalPackage> results = new List<IInternalPackage>();

            int i = 0;

            foreach (var packageVersion in packages)
            {
                var currentVersion = packageVersion.Version;
                var matchedPackages = (IEnumerable<IInternalPackage>)ds.Where(pkg => pkg.PackageId == packageVersion.Id).OrderBy(pkg => pkg.Version).ToList();

                if (matchedPackages.Any() && targetFrameworkValues.Any())
                {
                   matchedPackages = matchedPackages.Where(pkg => targetFrameworkValues.Any(fwk => VersionUtility.IsCompatible(fwk, pkg.GetSupportedFrameworks())));
                }

                matchedPackages = matchedPackages.Where(pkg => currentVersion.CompareTo(pkg.GetSemanticVersion()) < 0);

                if (versionSpecs.Any() && versionSpecs[i] != null)
                {
                    matchedPackages = matchedPackages.Where(pkg => versionSpecs[i].Satisfies(pkg.GetSemanticVersion()));
                }

                if (includeAllVersions)
                {
                    results.AddRange(matchedPackages);
                }
                else
                {
                    var latest = matchedPackages.LastOrDefault();
                    if (latest != null)
                    {
                        results.Add(latest);
                    }
                }

                i++;
            }


            return results.AsQueryable();
        }

        private IVersionSpec CreateVersionSpec(string constraint, SemanticVersion currentVersion)
        {
            if (!string.IsNullOrWhiteSpace(constraint))
            {
                return VersionUtility.ParseVersionSpec(constraint);
            }

            return new VersionSpec { MinVersion = currentVersion, IsMinInclusive = false };
        }

        public class PackageVersion
        {
            public string Id { get; set; }
            public SemanticVersion Version { get; set; }
        }
    }
}