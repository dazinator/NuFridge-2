using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Services;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using System.Xml;
using System.Xml.Serialization;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Linq2Rest;
using LinqToQuerystring;
using LinqToQuerystring.Nancy;
using LinqToQuerystring.TreeNodes;
using LinqToQuerystring.TreeNodes.Base;
using Microsoft.Data.Edm.Library;
using Microsoft.OData.Core;
using Microsoft.OData.Core.Atom;
using Nancy;
using Nancy.Authentication.Token;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Security;
using Newtonsoft.Json;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Diagnostics;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.Responses;
using NuGet;
using IFileSystem = NuFridge.Shared.Server.FileSystem.ILocalFileSystem;
using OrderByNode = LinqToQuerystring.TreeNodes.OrderByNode;


namespace NuFridge.Shared.Server.Web
{
      public class CustomNancyModule : NancyModule
  {
          public CustomNancyModule(ITokenizer tokenizer, InternalPackageRepositoryFactory packageRepositoryFactory, IFileSystem fileSystem, IStore store, IHomeConfiguration home)
          {
              Post["api/signin"] = p =>
              {
                  SignInRequest signInRequest;

                  try
                  {
                      signInRequest = this.Bind<SignInRequest>();
                  }
                  catch (Exception ex)
                  {
                      return null;
                  }


                  var userIdentity = UserDatabase.ValidateUser(signInRequest);

                  if (userIdentity == null)
                  {
                      return HttpStatusCode.Unauthorized;
                  }

                  var token = tokenizer.Tokenize(userIdentity, Context);

                  return new
                  {
                      Token = token,
                  };
              };

              Get["api/dashboard"] = p =>
              {
                  this.RequiresAuthentication();

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var feedsCount = transaction.Query<Feed>().Count();
                      var usersCount = transaction.Query<User>().Count();
                      var packagesCount = transaction.Query<InternalPackage>().Count();

                      return new
                      {
                          feedCount = feedsCount,
                          userCount = usersCount,
                          packageCount = packagesCount
                      };
                  }
              };

              Get["api/stats/feedpackagecount"] = p =>
              {
                  this.RequiresAuthentication();

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var model = new FeedPackageCountStatistic(transaction).GetModel();

                      return model;
                  }
              };

              Get["api/stats/feeddownloadcount"] = p =>
              {
                  this.RequiresAuthentication();

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var model = new FeedDownloadCountStatistic(transaction).GetModel();

                      return model;
                  }
              };

              //Get a list of feeds
              Get["api/feeds"] = p =>
              {
                  this.RequiresAuthentication();

                  int page = int.Parse(Request.Query["page"]);
                  int pageSize = int.Parse(Request.Query["pageSize"]);
                  int totalResults;

                  using (ITransaction transaction = store.BeginTransaction())
                  {

                      var feeds = transaction.Query<Feed>().ToList(pageSize*page, pageSize, out totalResults);

                      var totalPages = (int)Math.Ceiling((double)totalResults / pageSize);

                      return new
                      {
                          TotalCount = totalResults,
                          TotalPages = totalPages,
                          Results = feeds
                      };
                  }
              };

              //Search for a feed by name
              Get["api/feeds/search"] = p =>
              {
                  this.RequiresAuthentication();

                  FeedSearchResponse response = new FeedSearchResponse();

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      string name = Request.Query.name;

                      int totalResults;
                      var feeds = transaction.Query<Feed>().Where("Name like @feedName").Parameter("feedName", "%" + name + "%").ToList(0, 10, out totalResults);

                      var category = new FeedSearchResponse.Category("Default");
                      response.Results.Add(category);

                      string rootUrl = Request.Url.SiteBase + "/#feeds/view/{0}";

                      foreach (var feed in feeds)
                      {
                          category.Feeds.Add(new FeedSearchResponse.Category.FeedResult(feed.Name, string.Format(rootUrl, feed.Id)));
                      }
                  }

                  return response;
              };

              //Get a feed
              Get["api/feeds/{id}"] = p =>
              {
                  this.RequiresAuthentication();

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      int feedId = int.Parse(p.id);

                      return transaction.Query<Feed>().Where("Id = @feedId").Parameter("feedId", feedId).First();
                  }
              };

              //Insert a feed
              Post["api/feeds"] = p =>
              {
                  this.RequiresAuthentication();

                  Feed feed;

                  try
                  {
                      feed = this.Bind<Feed>();

                      ITransaction transaction = store.BeginTransaction();

                      var existingFeedExists =
                          transaction.Query<Feed>().Where("Name = @feedName").Parameter("feedName", feed.Name).Count() >
                          0;

                      if (existingFeedExists)
                      {
                          return HttpStatusCode.Conflict;
                      }

                      transaction.Insert(feed);
                      transaction.Commit();
                      transaction.Dispose();

                      transaction = store.BeginTransaction();

                      feed =
                          transaction.Query<Feed>()
                              .Where("Name = @feedName")
                              .Parameter("feedName", feed.Name)
                              .First();

                      var appFolder = home.InstallDirectory;
                      var feedFolder = Path.Combine(appFolder, "Feeds", feed.Id.ToString());

                      FeedConfiguration config = new FeedConfiguration
                      {
                          FeedId = feed.Id,
                          PackagesDirectory = feedFolder
                      };

                      transaction.Insert(config);
                      transaction.Commit();
                      transaction.Dispose();
                  }
                  catch (Exception ex)
                  {
                      return  HttpStatusCode.InternalServerError;
                  }


                  return feed;
              };

              //Update a feed
              Put["api/feeds/{id}"] = p =>
              {
                  this.RequiresAuthentication();

                  Feed feed;

                  try
                  {
                      int feedId = int.Parse(p.id);

                      feed = this.Bind<Feed>();

                      if (feedId != feed.Id)
                      {
                          return HttpStatusCode.BadRequest;
                      }

                      ITransaction transaction = store.BeginTransaction();

                      var existingFeedExists =
                          transaction.Query<Feed>().Where("Id = @feedId").Parameter("feedId", feedId).Count() >
                          0;

                      if (!existingFeedExists)
                      {
                          return HttpStatusCode.NotFound;
                      }

                      transaction.Update(feed);
                      transaction.Commit();
                      transaction.Dispose();
                  }
                  catch (Exception ex)
                  {
                      return HttpStatusCode.InternalServerError;
                  }


                  return feed;
              };

              //Delete a feed
              Delete["api/feeds/{id}"] = p =>
              {
                  this.RequiresAuthentication();

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      int feedId = int.Parse(p.id);

                      var feed = transaction.Query<Feed>().Where("Id = @feedId").Parameter("feedId", feedId).First();

                      if (feed == null)
                      {
                          return HttpStatusCode.NotFound;
                      }

                      var config = transaction.Query<FeedConfiguration>().Where("FeedId = @feedId").Parameter("feedId", feedId).First();

                      transaction.Delete(feed);
                      transaction.Delete(config);

                      if (Directory.Exists(config.PackagesDirectory))
                      {
                          try
                          {
                              Directory.Delete(config.PackagesDirectory);
                          }
                          catch(Exception ex)
                          {
                              transaction.Rollback();

                              //TODO standardized error responses
                              var response = Response.AsText(ex.Message);

                              response.StatusCode = HttpStatusCode.InternalServerError;

                              return response;
                          }
                      }

                      transaction.Commit();
                  }

                  //TODO responses
                  return Response.AsJson(new object());
              };

              //Register an account
              Post["api/account/register"] = p =>
              {
                  return null;
              };

              //Get an account
              Get["api/account/{id}"] = p =>
              {
                  this.RequiresAuthentication();

                  return null;
              };

              //Get diagnostic information
              Get["api/diagnostics"] = p =>
              {
                  this.RequiresAuthentication();

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var model = new SystemInformationStatistic(transaction).GetModel();

                      return model;
                  }
              };

              //Working
              Get["feeds/{feed}/api/v2/package/{id}/{version}"] = RedirectToDownloadPackage(packageRepositoryFactory, store);
              Get["feeds/{feed}/packages/{id}/{version}"] = DownloadPackage(packageRepositoryFactory, store);

              
              //Working
              Post["feeds/{feed}/api/v2/package/{id?}/{version?}"] = UploadPackage(packageRepositoryFactory, fileSystem, store);
              Post["feeds/{feed}/api/v2/package"] = UploadPackage(packageRepositoryFactory, fileSystem, store);

              //Working
              Put["feeds/{feed}/api/v2/package/{id?}/{version?}"] = UploadPackage(packageRepositoryFactory, fileSystem, store);
              Put["feeds/{feed}/api/v2/package"] = UploadPackage(packageRepositoryFactory, fileSystem, store);

              //Working
              Delete["feeds/{feed}/api/v2/package/{id?}/{version?}"] = DeletePackage(packageRepositoryFactory, store);

              //Working
              Get["feeds/{feed}/api/v2/Search()"] = p => ProcessODataRequest(packageRepositoryFactory, store, p);
              Get["feeds/{feed}/api/v2/odata/Search()"] = p => ProcessODataRequest(packageRepositoryFactory, store, p);

              //Working
              Get["feeds/{feed}/api/v2/Packages()"] = p => ProcessODataRequest(packageRepositoryFactory, store, p);
              Get["feeds/{feed}/api/v2/odata/Packages()"] = p => ProcessODataRequest(packageRepositoryFactory, store, p);

              //Not working
              Get["feeds/{feed}/api/v2/FindPackagesById()"] = p => ProcessODataFindPackagesByIdRequest(packageRepositoryFactory, store, p);
              Get["feeds/{feed}/api/v2/odata/FindPackagesById()"] = p => ProcessODataFindPackagesByIdRequest(packageRepositoryFactory, store, p);

              //Not working
              Get["feeds/{feed}/api/v2/GetUpdates()"] = p =>
              {
                  return null;
              };

              //Not working - odata url is for legacy feeds
              Get["feeds/{feed}/api/v2/odata/GetUpdates()"] = p =>
              {
                  return null;
              };

              //Not working
              Get["feeds/{feed}/api/v2/package-ids"] = p => View["index"];

              //Not working
              Get["feeds/{feed}/api/v2/package-versions/{packageId}"] = GetPackageVersions(packageRepositoryFactory, store);
          }

          private dynamic ProcessODataFindPackagesByIdRequest(InternalPackageRepositoryFactory packageRepositoryFactory, IStore store, dynamic p)
          {
                            string feedName = p.feed;

              using (ITransaction transaction = store.BeginTransaction())
              {
                  Feed feed =
                      transaction.Query<Feed>()
                          .Where("Name = @feedName")
                          .Parameter("feedName", feedName)
                          .First();

                  IDictionary<string, object> queryDictionary = Context.Request.Query;
                  NameValueCollection collection = new NameValueCollection();

                  foreach (var o in queryDictionary)
                  {
                      if (o.Key.StartsWith("$"))
                      collection.Add(o.Key, o.Value.ToString());
                  }

                  var joined = string.Join("&", queryDictionary.Select(o => o.Key + "=" + o.Value));
                  var queryString = !string.IsNullOrEmpty(joined) ? "?" + joined : string.Empty;

                  if (queryString.StartsWith("?"))
                  {
                      queryString = queryString.Substring(1);
                  }

                  var odataQueries = queryString.Split('&').Where(o => o.StartsWith("$")).ToList();

                  var odataQuerystring = Uri.UnescapeDataString(string.Join("&", odataQueries.ToArray()));

                  var input = new ANTLRReaderStream(new StringReader(odataQuerystring));
                  var lexer = new LinqToQuerystringLexer(input);
                  var tokStream = new CommonTokenStream(lexer);

                  var parser = new LinqToQuerystringParser(tokStream)
                  {
                      TreeAdaptor = new TreeNodeFactory(typeof(IWebPackage), false)
                  };

                  var result = parser.prog();


                  string packageId = queryDictionary.ContainsKey("id")
                      ? queryDictionary["id"].ToString()
                      : string.Empty;

                  string orderType = null;
                  string orderProperty = null;

                  string filterOp = null;
                  string filterProp = null;
                  string filterValue = null;

                  

                                    var tree = result.Tree as CommonTree;
                  if (tree != null)
                  {
                      var children = tree.Children.Cast<TreeNode>().ToList();
                      children.Sort();



                      var selectNode = children.FirstOrDefault(o => o is SelectNode);
                      if (selectNode != null && selectNode.Children != null)
                      {
                          List<string> selectProps = selectNode.Children.Select(child => child.Text).ToList();
                      }

                      var orderByNode = children.FirstOrDefault(o => o is OrderByNode);
                      if (orderByNode != null)
                      {
                          if (orderByNode.Children != null && orderByNode.Children.Any())
                          {
                              if (orderByNode.Children[0].ChildCount == 1)
                              {
                                  orderType = orderByNode.Children[0].Text;
                                  orderProperty = orderByNode.Children[0].GetChild(0).Text;
                              }
                              else
                              {
                                  throw new NotImplementedException();
                              }
                          }
                      }

                      var filterNode = children.FirstOrDefault(o => o is FilterNode);
                      if (filterNode != null && filterNode.Children != null && filterNode.Children.Any())
                      {
                          if (filterNode.Children[0].ChildCount == 2)
                          {
                              filterOp = filterNode.Children[0].Text;
                              filterProp = filterNode.Children[0].GetChild(0).Text;
                              filterValue = filterNode.Children[0].GetChild(1).Text;
                          }
                          else
                          {
                              throw new NotImplementedException();
                          }
                      }
                  }

                  var packageRepository = packageRepositoryFactory.Create(feed.Id);

                  var baseAddress = Request.Url.Scheme + "://" + Request.Url.HostName + ":" + Request.Url.Port + "/feeds/" + feedName + "/api/v2";

                  var packages = packageRepository.GetWebPackages(transaction, filterOp, filterProp, filterValue, orderType, orderProperty, "", "", "");

                  var filteredSource = packages.Filter(collection);

                  var stream = ODataPackages.CreatePackagesStream(baseAddress, packageRepository, baseAddress, packages, feed.Id, 0, int.MaxValue);

                  StreamReader reader = new StreamReader(stream);
                  string text = reader.ReadToEnd();

                  return new Response()
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



          private dynamic ProcessODataRequest(InternalPackageRepositoryFactory packageRepositoryFactory, IStore store, dynamic p)
          {
              string feedName = p.feed;

              using (ITransaction transaction = store.BeginTransaction())
              {
                  Feed feed =
                      transaction.Query<Feed>()
                          .Where("Name = @feedName")
                          .Parameter("feedName", feedName)
                          .First();


                  IDictionary<string, object> queryDictionary = Context.Request.Query;

                  var joined = string.Join("&", queryDictionary.Select(o => o.Key + "=" + o.Value));
                  var queryString = !string.IsNullOrEmpty(joined) ? "?" + joined : string.Empty;

                  if (queryString.StartsWith("?"))
                  {
                      queryString = queryString.Substring(1);
                  }

                  var odataQueries = queryString.Split('&').Where(o => o.StartsWith("$")).ToList();

                  var odataQuerystring = Uri.UnescapeDataString(string.Join("&", odataQueries.ToArray()));

                  var input = new ANTLRReaderStream(new StringReader(odataQuerystring));
                  var lexer = new LinqToQuerystringLexer(input);
                  var tokStream = new CommonTokenStream(lexer);

                  var parser = new LinqToQuerystringParser(tokStream)
                  {
                      TreeAdaptor = new TreeNodeFactory(typeof (IWebPackage), false)
                  };

                  var result = parser.prog();


                  string searchTerm = queryDictionary.ContainsKey("searchTerm")
                      ? queryDictionary["searchTerm"].ToString()
                      : string.Empty;

                  string targetFramework = queryDictionary.ContainsKey("targetFramework")
                      ? queryDictionary["targetFramework"].ToString()
                      : string.Empty;

                  string includePrerelease = queryDictionary.ContainsKey("includePrerelease")
                      ? queryDictionary["includePrerelease"].ToString()
                      : string.Empty;

                  int skip = 0;
                  int take = 15;
                  string filterOp = null;
                  string filterProp = null;
                  string filterValue = null;
                  string orderType = null;
                  string orderProperty = null;

                  var tree = result.Tree as CommonTree;
                  if (tree != null)
                  {
                      var children = tree.Children.Cast<TreeNode>().ToList();
                      children.Sort();

                      var selectNode = children.FirstOrDefault(o => o is SelectNode);
                      if (selectNode != null && selectNode.Children != null)
                      {
                          List<string> selectProps = selectNode.Children.Select(child => child.Text).ToList();
                      }

                      var skipNode = children.FirstOrDefault(o => o is SkipNode);
                      if (skipNode != null && skipNode.Children != null && skipNode.Children.Any())
                      {
                          if (!int.TryParse(skipNode.Children[0].Text, out skip))
                          {
                              skip = 0;
                          }
                      }

                      var orderByNode = children.FirstOrDefault(o => o is OrderByNode);
                      if (orderByNode != null)
                      {
                          if (orderByNode.Children != null && orderByNode.Children.Any())
                          {
                              if (orderByNode.Children[0].ChildCount == 1)
                              {
                                  orderType = orderByNode.Children[0].Text;
                                  orderProperty = orderByNode.Children[0].GetChild(0).Text;
                              }
                              else
                              {
                                  throw new NotImplementedException();
                              }
                          }
                      }

                      var filterNode = children.FirstOrDefault(o => o is FilterNode);
                      if (filterNode != null && filterNode.Children != null && filterNode.Children.Any())
                      {
                          if (filterNode.Children[0].ChildCount == 2)
                          {
                              filterOp = filterNode.Children[0].Text;
                              filterProp = filterNode.Children[0].GetChild(0).Text;
                              filterValue = filterNode.Children[0].GetChild(1).Text;
                          }
                          else
                          {
                              throw new NotImplementedException();
                          }
                      }

                      var topNode = children.FirstOrDefault(o => o is TopNode);
                      if (topNode != null && topNode.Children != null && topNode.Children.Any())
                      {
                          if (!int.TryParse(topNode.Children[0].Text, out take))
                          {
                              take = 0;
                          }
                      }

                      var inlineCountNode = children.FirstOrDefault(o => o is InlineCountNode);
                      if (inlineCountNode != null)
                      {
                          if (inlineCountNode.Children != null && inlineCountNode.Children.Any())
                          {
                              throw new NotImplementedException();
                          }
                      }
                  }

                  var packageRepository = packageRepositoryFactory.Create(feed.Id);

                  var baseAddress = Request.Url.Scheme + "://" + Request.Url.HostName + ":" + Request.Url.Port + "/feeds/" + feedName + "/api/v2";

                  var packages = packageRepository.GetWebPackages(transaction,  filterOp, filterProp, filterValue, orderType, orderProperty, searchTerm, targetFramework, includePrerelease);

                  var stream = ODataPackages.CreatePackagesStream(baseAddress, packageRepository, baseAddress, packages, feed.Id, skip, take);

                  StreamReader reader = new StreamReader(stream);
                  string text = reader.ReadToEnd();

                  return new Response()
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

          private Func<dynamic, dynamic> GetPackageVersions(InternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
          {
              return parameters =>
              {
                  string packageId = parameters.packageId;
                  string feedName = parameters.feed;

                  if (string.IsNullOrWhiteSpace(packageId))
                  {
                      return new Response {StatusCode = HttpStatusCode.BadRequest};
                  }


                  int feedId;

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var feed =
                          transaction.Query<Feed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                      feedId = feed.Id;


                      int total;

                      var packageRepository = packageRepositoryFactory.Create(feed.Id);

                      var versions = packageRepository.GetVersions(transaction, packageId, false);

                      return Response;
                  }
              };
          }

          private Func<dynamic, dynamic> RedirectToDownloadPackage(InternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
          {
              return parameters =>
              {
                  string id = parameters.id;
                  string version = parameters.version;
                  string feedName = parameters.feed;

                  int feedId;

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var feed = transaction.Query<Feed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                      feedId = feed.Id;
                  }

                  var packageRepository = packageRepositoryFactory.Create(feedId);

                  IPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


                  if (package == null)
                  {
                      var errorResponse = Response.AsText(string.Format("Package {0} version {1} not found.", id, version));
                      errorResponse.StatusCode = HttpStatusCode.NotFound;
                      return errorResponse;
                  }

                  var response = new Response();

   
                  var baseAddress = Request.Url.Scheme + "://" + Request.Url.HostName + ":" + Request.Url.Port + "/feeds/" + feedName;

                  var location = string.Format("{0}/packages/{1}/{2}", baseAddress, package.Id, package.Version);
              
                  response.Headers.Add("Location", location);

                  response.Contents = delegate(Stream stream)
                  {
                      var writer = new StreamWriter(stream) { AutoFlush = true };
                      writer.Write("<html><head><title>Object moved</title></head><body><h2>Object moved to <a href=\"{0}\">here</a>.</h2></body></html>", location); 
                  };

                  response.ContentType = "text/html";
                  response.StatusCode = HttpStatusCode.Found;

                  return response;
              };
          }

          private Func<dynamic, dynamic> DownloadPackage(InternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
          {
              return parameters =>
              {
                  string id = parameters.id;
                  string version = parameters.version;
                  string feedName = parameters.feed;

                  int feedId;

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var feed =
                          transaction.Query<Feed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                      feedId = feed.Id;
                  }

                  var packageRepository = packageRepositoryFactory.Create(feedId);

                  IPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


                  if (package == null)
                  {
                      var response = Response.AsText(string.Format("Package {0} version {1} not found.", id, version));
                      response.StatusCode = HttpStatusCode.NotFound;
                      return response;
                  }

                  packageRepository.IncrementDownloadCount(package);

                  Response result;

                  var stream = packageRepository.GetPackageRaw(id, new SemanticVersion(version));

                  if (Request.Method == HttpMethod.Get.Method)
                  {
                      result = Response.FromStream(stream, "application/zip");

                      result.Headers.Add("Content-Length", stream.Length.ToString());

                      using (var md5 = MD5.Create())
                      {
                          result.Headers.Add("Content-MD5", BitConverter.ToString(md5.ComputeHash(stream)).Replace("-","").ToLower());
                          stream.Seek(0, SeekOrigin.Begin);
                      }
                  }
                  else
                  {
                      result = global::Nancy.Response.NoBody;
                  }

                  result.StatusCode = HttpStatusCode.OK;

                  return result;
              };
          }

          private Func<dynamic, dynamic> DeletePackage(InternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
          {
              return parameters =>
              {
                  string id = parameters.id;
                  string version = parameters.version;
                  string feedName = parameters.feed;

                  int feedId;

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var feed = transaction.Query<Feed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                      feedId = feed.Id;
                  }

                  var packageRepository = packageRepositoryFactory.Create(feedId);

                  IPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


                  if (package == null)
                  {
                      var response = Response.AsText(string.Format("Package {0} version {1} not found.", id, version));
                      response.StatusCode = HttpStatusCode.NotFound;
                      return response;
                  }

                  packageRepository.RemovePackage(package);

                  return new Response {StatusCode = HttpStatusCode.Created};
              };
          }

          private Func<dynamic, dynamic> UploadPackage(InternalPackageRepositoryFactory packageRepositoryFactory, IFileSystem fileSystem, IStore store)
          {
              return parameters =>
              {
                  var file = Request.Files.FirstOrDefault();
                  string feedName = parameters.feed;

                  if (file == null)
                  {
                      var response = Response.AsText("Must provide package with valid id and version.");
                      response.StatusCode = HttpStatusCode.BadRequest;
                      return response;
                  }

                  int feedId;

                  using (ITransaction transaction = store.BeginTransaction())
                  {
                      var feed = transaction.Query<Feed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                      feedId = feed.Id;
                  }

                  string temporaryFilePath;
                  using (var stream = fileSystem.CreateTemporaryFile(".nupkg", out temporaryFilePath))
                  {
                      file.Value.CopyTo(stream);
                  }

                  try
                  {
                      IPackage package = FastZipPackage.Open(temporaryFilePath, new CryptoHashProvider());

                      if (string.IsNullOrWhiteSpace(package.Id) || package.Version == null)
                      {
                          var response = Response.AsText("Must provide package with valid id and version.");
                          response.StatusCode = HttpStatusCode.BadRequest;
                          return response;
                      }

                      IInternalPackage latestAbsoluteVersionPackage = null;
                      IInternalPackage latestVersionPackage = null;

                      using (ITransaction transaction = store.BeginTransaction())
                      {
                          var packageRepository = packageRepositoryFactory.Create(feedId);

                          var versionsOfPackage = packageRepository.GetVersions(transaction, package.Id, true).ToList();

                          if (versionsOfPackage.Any())
                          {
                              foreach (var versionOfPackage in versionsOfPackage)
                              {
                                  if (versionOfPackage.IsAbsoluteLatestVersion)
                                  {
                                      latestAbsoluteVersionPackage = (IInternalPackage)versionOfPackage;
                                  }
                                  if (versionOfPackage.IsLatestVersion)
                                  {
                                      latestVersionPackage = (IInternalPackage)versionOfPackage;
                                  }

                                  if (latestVersionPackage != null && latestAbsoluteVersionPackage != null)
                                  {
                                      break;
                                  }
                              }
                          }

                          bool isUploadedPackageAbsoluteLatestVersion = true;
                          bool isUploadedPackageLatestVersion = true;

                          if (latestAbsoluteVersionPackage != null)
                          {
                              if (package.Version.CompareTo(latestAbsoluteVersionPackage.Version) <= 0)
                              {
                                  isUploadedPackageAbsoluteLatestVersion = false;
                              }
                          }

                          if (latestVersionPackage != null)
                          {
                              if (package.Version.CompareTo(latestVersionPackage.Version) <= 0)
                              {
                                  isUploadedPackageLatestVersion = false;
                              }
                              else
                              {
                                  if (!package.IsReleaseVersion())
                                  {
                                      isUploadedPackageLatestVersion = false;
                                  }
                              }
                          }
                          else
                          {
                              if (!package.IsReleaseVersion())
                              {
                                  isUploadedPackageLatestVersion = false;
                              }
                          }

                          if (isUploadedPackageAbsoluteLatestVersion && latestAbsoluteVersionPackage != null)
                          {
                              latestAbsoluteVersionPackage.IsAbsoluteLatestVersion = false;
                              transaction.Update(latestAbsoluteVersionPackage);
                          }

                          if (isUploadedPackageLatestVersion && latestVersionPackage != null)
                          {
                              latestVersionPackage.IsLatestVersion = false;
                              transaction.Update(latestVersionPackage);
                          }

                          transaction.Commit();

                          packageRepository.AddPackage(package, isUploadedPackageAbsoluteLatestVersion, isUploadedPackageLatestVersion);
                      }
                  }
                  finally
                  {
                      if (File.Exists(temporaryFilePath))
                      {
                          fileSystem.DeleteFile(temporaryFilePath);
                      }
                  }

                  return new Response {StatusCode = HttpStatusCode.Created};
              };
          }
  }
}
