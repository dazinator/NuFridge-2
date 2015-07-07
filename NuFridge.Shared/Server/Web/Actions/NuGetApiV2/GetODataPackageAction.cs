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
using Microsoft.Data.OData;
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
    public class GetODataPackageAction : IAction
    {
        protected readonly IInternalPackageRepositoryFactory PackageRepositoryFactory;
        protected readonly IStore Store;
        private readonly IWebPortalConfiguration _portalConfig;

        public GetODataPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IWebPortalConfiguration portalConfig)
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

            using (var dbContext = new DatabaseContext(Store))
            {
                IQueryable<IInternalPackage> ds = dbContext.Packages.AsNoTracking().AsQueryable();

                ds = ds.Where(pk => pk.FeedId == feed.Id);
                ds = ds.Where(pk => pk.Listed);

                string packageId = parameters.PackageId;
                string packageVersion = parameters.PackageVersion;

                if (!string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(packageVersion))
                {
                    ds = ds.Where(pk => pk.PackageId == packageId && pk.Version == packageVersion);
                }

                var package = ds.FirstOrDefault();

                if (package == null)
                {
                    return new Response() {StatusCode = HttpStatusCode.NotFound};
                }

                return ProcessResponse(module, feed, package);
            }
        }

        protected virtual dynamic ProcessResponse(INancyModule module, IFeed feed, IInternalPackage package)
        {
            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var baseAddress = string.Format("{0}{1}feeds/{2}/api/v2/", _portalConfig.ListenPrefixes, endsWithSlash ? "" : "/", feed.Name);

            NuGetODataModelBuilderODataPackage builder = new NuGetODataModelBuilderODataPackage();
            builder.Build();

            var writerSettings = new ODataMessageWriterSettings()
            {
                Indent = true,
                CheckCharacters = false,
                BaseUri = new Uri(baseAddress),
                Version = ODataVersion.V3
            };

            writerSettings.SetContentType(ODataFormat.Atom);

            var responseMessage = new MemoryResponseMessage();
            var writer = new ODataMessageWriter(responseMessage, writerSettings);

            var entryWriter = writer.CreateODataEntryWriter();

            entryWriter.WriteStart(ODataPackages.MapPackageToEntry(baseAddress, new ODataPackage(package)));
            entryWriter.WriteEnd();

            var msgStream = responseMessage.GetStream();

            msgStream.Seek(0, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(msgStream);
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