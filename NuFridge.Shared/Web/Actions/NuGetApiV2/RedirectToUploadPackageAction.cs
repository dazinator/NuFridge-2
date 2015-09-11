using System.IO;
using Nancy;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Web.Actions.NuGetApiV2
{
    public class RedirectToUploadPackageAction : IAction
    {
        private readonly IWebPortalConfiguration _portalConfig;
        private readonly IFeedService _feedService;

        public RedirectToUploadPackageAction(IWebPortalConfiguration portalConfig, IFeedService feedService)
        {
            _portalConfig = portalConfig;
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;

            Feed feed = _feedService.Find(feedName, false);

            if (feed == null)
            {
                var errorResponse = module.Response.AsText($"Feed does not exist called {feedName}.");
                errorResponse.StatusCode = HttpStatusCode.NotFound;
                return errorResponse;
            }

            var response = new Response();

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var location = $"{_portalConfig.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feedName}/api/v2/package";

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