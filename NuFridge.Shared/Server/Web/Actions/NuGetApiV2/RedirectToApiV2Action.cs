using System.IO;
using Nancy;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
   public class RedirectToApiV2Action : IAction
    {
       private readonly IWebPortalConfiguration _portalConfig;

       public RedirectToApiV2Action(IWebPortalConfiguration portalConfig)
       {
           _portalConfig = portalConfig;
       }

       public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;

            var response = new Response();

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var location = $"{_portalConfig.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feedName}/api/v2";

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