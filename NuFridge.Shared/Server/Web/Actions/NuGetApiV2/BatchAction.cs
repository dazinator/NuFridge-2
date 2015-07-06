using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nancy;
using NuFridge.Shared.Server.Web.Batch;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class BatchAction : IAction
    {
        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            var contentType = module.Request.Headers["content-type"].First();
            var mimeType = contentType.Split(';').First();
            if (mimeType != "multipart/batch" && mimeType != "multipart/mixed")
                return (int)HttpStatusCode.BadRequest;



            var multipartBoundry = Regex.Match(contentType, @"boundary=(?<token>[^\n\; ]*)").Groups["token"].Value.Replace("\"", "");
            if (string.IsNullOrEmpty(multipartBoundry))
                return (int)HttpStatusCode.BadRequest;

            var multipart = new HttpMultipart(module.Request.Body, multipartBoundry);

            foreach (var boundry in multipart.GetBoundaries())
            {
                using (var httpRequest = new StreamReader(boundry.Value))
                {
                    var requestStr = httpRequest.ReadToEnd();
                    requestStr += "User-Agent: " + module.Request.Headers["User-Agent"].FirstOrDefault();
                    requestStr += "\r" + "Host: " + module.Request.Url.HostName;
                    requestStr += @"
";
                    var rawRequestStream = new MemoryStream(Encoding.UTF8.GetBytes(requestStr));
                    var result = rawRequestStream.ReadAsRequest();
                    var t = result;
                }
            }

            return (int)HttpStatusCode.OK;
        }
    }
}