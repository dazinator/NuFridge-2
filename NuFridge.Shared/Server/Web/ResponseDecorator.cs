using System;
using System.Linq;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Web.Nancy;

namespace NuFridge.Shared.Server.Web
{
    public class ResponseDecorator
    {
        public static Func<NancyContext, string, Response> StaticContent(Func<NancyContext, string, Response> responsomatic, IWebPortalConfiguration portalConfiguration)
        {
            return (nancyContext, s) =>
            {
                if (nancyContext.Request.Url.IsSecure || !portalConfiguration.ListenPrefixes.Contains("https://"))
                    return DecorateWithHttpHeaders(NancyCompression.CompressStaticContent(responsomatic))(nancyContext, s);

                if (nancyContext.Request.Method == "GET")
                {
                    Url url1 = portalConfiguration.ListenPrefixes.Split(',').Select(prefix => new Url(prefix)).FirstOrDefault(url =>
                    {
                        if (url.IsSecure)
                            return url.HostName.Equals(nancyContext.Request.Url.HostName, StringComparison.InvariantCultureIgnoreCase);
                        return false;
                    });
                    if (url1 != null)
                        return (Response)new RedirectResponse(new Uri(url1).ToString());
                }
                return new Response {StatusCode = HttpStatusCode.BadRequest};
            };
        }


        private static Func<NancyContext, string, Response> DecorateWithHttpHeaders(Func<NancyContext, string, Response> responsomatic)
        {
            return (context, s) =>
            {
                Response response = responsomatic(context, s);
                if (response != null)
                {
                    response.WithHeader("Server", "NuFridge");
                    response.WithHeader("X-UA-Compatible", "IE=Edge,chrome=1");
                }
                return response;
            };
        }
    }
}