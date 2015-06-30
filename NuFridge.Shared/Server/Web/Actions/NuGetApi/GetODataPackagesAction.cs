using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Nancy;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.OData;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApi
{
    public class GetODataPackagesAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;

        public GetODataPackagesAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            IFeed feed;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                feed =
                    transaction.Query<IFeed>()
                        .Where("Name = @feedName")
                        .Parameter("feedName", feedName)
                        .First();
            }

            IDictionary<string, object> queryDictionary = module.Request.Query;

                if (queryDictionary.ContainsKey("$select"))
                {
                    queryDictionary.Remove("$select");
                }


                NuGetWebApiODataModelBuilder builder = new NuGetWebApiODataModelBuilder();
                builder.Build();

                var context = new ODataQueryContext(builder.Model, typeof(IInternalPackage));

                HttpMethod method = new HttpMethod(module.Request.Method);

                var url = module.Request.Url.SiteBase + module.Request.Url.Path;

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

                var request = new HttpRequestMessage(method, url);
                request.Properties.AddRange(queryDictionary);

                using (var dbContext = new DatabaseContext(_store))
                {
                    IQueryable<IInternalPackage> ds = dbContext.Packages.AsNoTracking().AsQueryable();


                    string searchTerm = queryDictionary.ContainsKey("searchTerm")
                        ? queryDictionary["searchTerm"].ToString()
                        : string.Empty;

                    //TODO
                    string targetFramework = queryDictionary.ContainsKey("targetFramework")
                        ? queryDictionary["targetFramework"].ToString()
                        : string.Empty;

                    string idSearch = queryDictionary.ContainsKey("id")
                        ? queryDictionary["id"].ToString()
                        : string.Empty;

                    if (!string.IsNullOrWhiteSpace(idSearch))
                    {
                        if (idSearch.StartsWith("'") && idSearch.EndsWith("'"))
                        {
                            idSearch = idSearch.Substring(1, idSearch.Length - 2);
                        }
                        ds = ds.Where(pk => pk.PackageId == idSearch);
                    }

                    bool includePrerelease = queryDictionary.ContainsKey("includePrerelease") && bool.Parse(queryDictionary["includePrerelease"].ToString());

                    ds = ds.Where(pk => pk.FeedId == feed.Id);

                    if (!includePrerelease)
                    {
                        ds = ds.Where(pk => !pk.IsPrerelease);
                    }

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        if (searchTerm.StartsWith("'") && searchTerm.EndsWith("'"))
                        {
                            searchTerm = searchTerm.Substring(1, searchTerm.Length - 2);
                        }
                        ds = ds.Where(pk => pk.PackageId.Contains(searchTerm));
                    }

                    ODataQueryOptions options = new ODataQueryOptions(context, request);

                    var settings = new ODataQuerySettings
                    {
                        PageSize = options.Top != null ? options.Top.Value : 15
                    };


                    ds = options.ApplyTo(ds, settings) as IQueryable<IInternalPackage>;


                    long? total = request.GetInlineCount();


                    var packageRepository = _packageRepositoryFactory.Create(feed.Id);

                    var baseAddress = module.Request.Url.Scheme + "://" + module.Request.Url.HostName + ":" + module.Request.Url.Port +
                                      "/feeds/" + feedName + "/api/v2";

                    var stream = ODataPackages.CreatePackagesStream(baseAddress, packageRepository, baseAddress,
                        ds, feed.Id, total.HasValue ? int.Parse(total.Value.ToString()) : 0);

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