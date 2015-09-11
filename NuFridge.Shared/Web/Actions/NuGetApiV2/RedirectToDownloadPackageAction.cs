using System.IO;
using Nancy;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet.Repository;
using NuGet;

namespace NuFridge.Shared.Web.Actions.NuGetApiV2
{
    public class RedirectToDownloadPackageAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IWebPortalConfiguration _portalConfig;
        private readonly IFeedService _feedService;

        public RedirectToDownloadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory,IWebPortalConfiguration portalConfig, IFeedService feedService)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _portalConfig = portalConfig;
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string id = parameters.id;
            string version = parameters.version;
            string feedName = parameters.feed;

            Feed feed = _feedService.Find(feedName, false);

            if (feed == null)
            {
                var errorResponse = module.Response.AsText($"Feed does not exist called {feedName}.");
                errorResponse.StatusCode = HttpStatusCode.NotFound;
                return errorResponse;
            }

            var packageRepository = _packageRepositoryFactory.Create(feed.Id);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


            if (package == null)
            {
                var errorResponse = module.Response.AsText($"Package {id} version {version} not found.");
                errorResponse.StatusCode = HttpStatusCode.NotFound;
                return errorResponse;
            }

            var response = new Response();

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var location =
                $"{_portalConfig.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feedName}/packages/{package.Id}/{package.Version}";

            response.Headers.Add("Location", location);

            response.Contents = delegate(Stream stream)
            {
                var writer = new StreamWriter(stream) { AutoFlush = true };
                writer.Write("<html><head><title>Object moved</title></head><body><h2>Object moved to <a href=\"{0}\">here</a>.</h2></body></html>", location);
            };

            response.ContentType = "text/html";
            response.StatusCode = HttpStatusCode.Found;

            return response;
        }
    }
}