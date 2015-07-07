using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Nancy;
using NuFridge.Shared.Server.Web.Batch;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class BatchAction : IAction
    {
        private readonly INancyEngine _engine;
        private readonly StringBuilder _responseBuilder;

        public BatchAction(INancyEngine engine)
        {
            _engine = engine;
            _responseBuilder = new StringBuilder();
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            var contentType = module.Request.Headers["content-type"].First();
            var mimeType = contentType.Split(';').First();
            if (mimeType != "multipart/batch" && mimeType != "multipart/mixed")
                return (int)HttpStatusCode.BadRequest;

            var multipartBoundry = Regex.Match(contentType, @"boundary=(?<token>[^\n\; ]*)").Groups["token"].Value.Replace("\"", "");
            if (string.IsNullOrEmpty(multipartBoundry))
                return (int)HttpStatusCode.BadRequest;



            var multipart = new HttpMultipart(module.Request.Body, multipartBoundry);

            IDictionary<string, IEnumerable<string>> headers = module.Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


            string batchResponseGuid = Guid.NewGuid().ToString().ToLower();

            var response = new Response
            {
                ContentType = string.Format("{0}; boundary=batchresponse_{1}", mimeType, batchResponseGuid),
                StatusCode = HttpStatusCode.Accepted
            };

            _responseBuilder.Append(string.Format("--batchresponse_{0}\r\n", batchResponseGuid));
            _responseBuilder.Append("Content-Type: application/http\r\n");
            _responseBuilder.Append("Content-Transfer-Encoding: binary\r\n\r\n");

            foreach (var boundry in multipart.GetBoundaries())
            {
                using (var httpRequest = new StreamReader(boundry.Value))
                {
                    var httpRequestText = httpRequest.ReadToEnd();

                    var rawRequestStream = new MemoryStream(Encoding.UTF8.GetBytes(httpRequestText));
                    var nancyRequest = rawRequestStream.ReadAsRequest(headers);

                    _engine.HandleRequest(nancyRequest, OnSuccess, OnError);
                }
            }

            _responseBuilder.Append(string.Format("\r\n--batchresponse_{0}--", batchResponseGuid));

            var responseText = _responseBuilder.ToString();
            var byteData = Encoding.UTF8.GetBytes(responseText);

            response.Headers.Add("Content-Length", byteData.Length.ToString());

            response.Contents = contentStream =>
            {
                contentStream.Write(byteData, 0, byteData.Length);
            };

            return response;
        }

        private void OnSuccess(NancyContext obj)
        {
            _responseBuilder.Append("HTTP/1.1 200 OK\r\n");
            _responseBuilder.Append(string.Format("Content-Type: {0}\r\n\r\n", obj.Response.ContentType));

            using (var stream = new MemoryStream())
            {
                obj.Response.Contents(stream);

                stream.Seek(0, SeekOrigin.Begin);

                using (var sr = new StreamReader(stream))
                {
                    var responseText = sr.ReadToEnd();

                    _responseBuilder.Append(responseText);
                }
            }
        }

        private void OnError(Exception exception)
        {

        }
    }
}