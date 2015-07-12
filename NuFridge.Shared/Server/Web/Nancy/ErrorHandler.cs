using System.Linq;
using System.Text;
using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses;
using Nancy.Responses.Negotiation;

namespace NuFridge.Shared.Server.Web.Nancy
{
    public class ErrorHandler : IStatusCodeHandler
    {

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var enumerable = context.Request.Headers.Accept;
            bool html = enumerable.Select(o => new MediaRange(o.Item1)).Any(it => it.Matches("text/html"));

            if (!html)
            {
                return;
            }

            var response = new TextResponse();

            string contents = "An error has occurred.";

            switch (statusCode)
            {
                    case HttpStatusCode.Unauthorized:
                    contents = "You are unauthorized to access this page.";
                    break;
                    case HttpStatusCode.Forbidden:
                    contents = "You are unauthorized to access this page.";
                    break;
                    case HttpStatusCode.InternalServerError:
                    contents = "There was a server error trying to access the page.";
                    break;
                    case HttpStatusCode.NotFound:
                    contents = "The requested page was not found.";
                    break;
                    case HttpStatusCode.BadRequest:
                    contents = "The request sent from the client was not valid.";
                    break;
            }

            response.Contents = stream =>
            {
                var data = Encoding.UTF8.GetBytes(contents);
                stream.Write(data, 0, data.Length);
            };

            context.Response = response;
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.NotFound
                     || statusCode == HttpStatusCode.InternalServerError
                     || statusCode == HttpStatusCode.Forbidden
                     || statusCode == HttpStatusCode.Unauthorized
                     || statusCode == HttpStatusCode.BadRequest;
        }
    }
}