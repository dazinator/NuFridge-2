using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.OData;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.OData;

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

            using (var dbContext = new DatabaseContext())
            {
                IQueryable<IInternalPackage> ds = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id);

                ds = ds.Where(pk => pk.Listed);

                string packageId = parameters.PackageId;
                string packageVersion = parameters.PackageVersion;

                if (!string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(packageVersion))
                {
                    ds = ds.Where(pk => pk.Id == packageId && pk.Version == packageVersion);
                }

                var package = ds.FirstOrDefault();

                if (package == null)
                {
                    return new Response {StatusCode = HttpStatusCode.NotFound};
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

            var writerSettings = new ODataMessageWriterSettings
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

            entryWriter.WriteStart(ODataPackages.MapPackageToEntry(baseAddress, new ODataPackage(package), new string[0]));
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
            using (var dbContext = new DatabaseContext())
            {
                feed =
                    dbContext.Feeds.AsNoTracking()
                        .FirstOrDefault(f => f.Name.Equals(feedName, StringComparison.InvariantCultureIgnoreCase));
            }
            return feed;
        }
    }
}