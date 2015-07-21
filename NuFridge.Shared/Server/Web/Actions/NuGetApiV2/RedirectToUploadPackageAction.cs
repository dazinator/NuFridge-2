using System.IO;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class RedirectToUploadPackageAction : IAction
    {
        private readonly IStore _store;
        private readonly IWebPortalConfiguration _portalConfig;

        public RedirectToUploadPackageAction(IStore store, IWebPortalConfiguration portalConfig)
        {
            _store = store;
            _portalConfig = portalConfig;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            string feedName = parameters.feed;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                if (feed == null)
                {
                    var errorResponse = module.Response.AsText("Feed does not exist.");
                    errorResponse.StatusCode = HttpStatusCode.BadRequest;
                    return errorResponse;
                }
            }


            var response = new Response();

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var location = string.Format("{0}{1}feeds/{2}/api/v2/package", _portalConfig.ListenPrefixes, endsWithSlash ? "" : "/", feedName);

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