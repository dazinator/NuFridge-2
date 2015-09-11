using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet.Repository;
using NuFridge.Shared.Security;
using NuFridge.Shared.Web.OData;
using NuGet;

namespace NuFridge.Shared.Web.Actions.NuGetApiV2
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

            if (module.Context.CurrentUser != null && module.Context.CurrentUser.IsAuthenticated())
            {
                module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanViewPackages });
            }

            string packageId = parameters.PackageId;
            string packageVersion = parameters.PackageVersion;

            if (!string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(packageVersion))
            {
                var package = _packageService.GetPackage(feed.Id, packageId, new SemanticVersion(packageVersion));

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

            var enumerable = module.Request.Headers.Accept;
            var ranges = enumerable.OrderByDescending(o => o.Item2).Select(o => new MediaRange(o.Item1)).ToList();

            bool isXmlResponse = false;

            foreach (var mediaRange in ranges)
            {
                if (mediaRange.Matches("application/xml"))
                {
                    isXmlResponse = true;
                }
            }

            if (!isXmlResponse)
            {
                writerSettings.SetMetadataDocumentUri(new Uri(baseAddress));
                writerSettings.SetContentType(ODataFormat.VerboseJson);
            }
            else
            {
                writerSettings.SetContentType(ODataFormat.Atom);
            }

            var responseMessage = new MemoryResponseMessage();
            var writer = new ODataMessageWriter(responseMessage, writerSettings);

            var entryWriter =
                writer.CreateODataEntryWriter(new EdmEntitySet(new EdmEntityContainer("NS", "CONTAINER"), "NAME",
                    new EdmEntityType("NS", "NAME")));

            entryWriter.WriteStart(ODataPackages.MapPackageToEntry(baseAddress, new ODataPackage(package), new string[0]));
            entryWriter.WriteEnd();

            var msgStream = responseMessage.GetStream();

            msgStream.Seek(0, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(msgStream);
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
    }
}