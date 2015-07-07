using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses;
using Nancy.Responses.Negotiation;
using Newtonsoft.Json;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Web
{
    public sealed class ErrorStatusCodeHandler : IStatusCodeHandler
    {
        private readonly Lazy<IWebPortalConfiguration> _portalConfiguration;

        public ErrorStatusCodeHandler(Lazy<IWebPortalConfiguration> portalConfiguration)
        {
            _portalConfiguration = portalConfiguration;
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            if (statusCode != (HttpStatusCode)404 && statusCode != (HttpStatusCode)500 && (statusCode != (HttpStatusCode)403 && statusCode != (HttpStatusCode)503) && statusCode != (HttpStatusCode)400)
                return statusCode == (HttpStatusCode)401;
            return true;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            if (context.Response is NotFoundResponse)
                context.Response = ErrorResponse.FromMessage(statusCode, "The resource you requested was not found.");
            if (context.Response == null)
                return;
            ErrorResponse errorResponse = context.Response as ErrorResponse;
            if (ShouldRenderFriendlyErrorPage(context) && errorResponse != null)
            {
                string basePath = context.Request.Url.BasePath;
                string baseUrl;
                if (string.IsNullOrWhiteSpace(basePath))
                    baseUrl = "/";
                else
                    baseUrl = "/" + basePath.Trim('/') + "/";
                HttpStatusCode httpStatusCode = statusCode;
                switch (httpStatusCode - 400)
                {
                    case 0:
                        context.Response = new ErrorHtmlPageResponse(statusCode, baseUrl)
                        {
                            Title = "Bad request",
                            Summary = errorResponse.ErrorMessage,
                            Details = errorResponse
                        };
                        break;
                    case (HttpStatusCode)1:
                        context.Response = new RedirectResponse(baseUrl + "app");
                        break;
                    case (HttpStatusCode)2:
                        break;
                    case (HttpStatusCode)3:
                        context.Response = new ErrorHtmlPageResponse(statusCode, baseUrl)
                        {
                            Title = "Permission",
                            Summary = errorResponse.ErrorMessage,
                            Details = errorResponse
                        };
                        break;
                    case (HttpStatusCode)4:
                        context.Response = new ErrorHtmlPageResponse(statusCode, baseUrl)
                        {
                            Title = "Not found",
                            Summary = errorResponse.ErrorMessage,
                            Details = errorResponse
                        };
                        break;
                    default:
                        if (httpStatusCode != (HttpStatusCode)500)
                        {
                            if (httpStatusCode == (HttpStatusCode)503)
                            {
                                context.Response = new ErrorHtmlPageResponse(statusCode, baseUrl)
                                {
                                    Title = "Please wait...",
                                    Summary = errorResponse.ErrorMessage,
                                    Details = errorResponse
                                };
                            }
                            break;
                        }
                        context.Response = new ErrorHtmlPageResponse(statusCode, baseUrl)
                        {
                            Title = "Sorry, something went wrong",
                            Summary = errorResponse.ErrorMessage,
                            Details = errorResponse
                        };
                        break;
                }
            }
        }

        private static bool ShouldRenderFriendlyErrorPage(NancyContext context)
        {
            using (List<MediaRange>.Enumerator enumerator = context.Request.Headers.Accept.OrderByDescending(o => o.Item2).Select(o => new MediaRange(o.Item1)).ToList().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MediaRange current = enumerator.Current;
                    if (current.Matches(MediaRange.FromString("application/json")) || current.Matches(MediaRange.FromString("text/json")))
                        return false;
                    if (current.Matches(MediaRange.FromString("text/html")))
                        return true;
                }
            }
            return false;
        }

        public class ErrorHtmlPageResponse : HtmlResponse
        {
            private readonly string _baseUrl;
            private static readonly string ErrorTemplate;

            public string Title { get; set; }

            public string Summary { get; set; }

            public ErrorResponse Details { get; set; }

            static ErrorHtmlPageResponse()
            {
                ErrorTemplate = "An error has occurred.";
            }

            public ErrorHtmlPageResponse(HttpStatusCode statusCode, string baseUrl)
                : base((HttpStatusCode)200, null, null, null)
            {
                _baseUrl = baseUrl;
                StatusCode = statusCode;
                ContentType = "text/html; charset=utf-8";
                Contents = Render;
            }

            private void Render(Stream stream)
            {
                string str = ErrorTemplate;
                using (StreamWriter streamWriter = new StreamWriter(stream))
                {
                    streamWriter.WriteLine(str);
                    streamWriter.Flush();
                }
            }

        }
    }
}
