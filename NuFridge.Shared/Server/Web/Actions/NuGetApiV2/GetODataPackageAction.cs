using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.OData;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
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
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public GetODataPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IWebPortalConfiguration portalConfig, IFeedService feedService, IPackageService packageService)
        {
            PackageRepositoryFactory = packageRepositoryFactory;
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

            string packageId = parameters.PackageId;
            string packageVersion = parameters.PackageVersion;

            if (!string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(packageVersion))
            {
                var package = _packageService.GetPackage(feed.Id, packageId, packageVersion);

                if (package == null || !package.Listed)
                {
                    return new Response {StatusCode = HttpStatusCode.NotFound};
                }

                return ProcessResponse(module, feed, package);
            }

            return new Response {StatusCode = HttpStatusCode.BadRequest};
        }

        protected virtual dynamic ProcessResponse(INancyModule module, IFeed feed, IInternalPackage package)
        {
            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var baseAddress = $"{_portalConfig.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feed.Name}/api/v2/";

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
    }
}