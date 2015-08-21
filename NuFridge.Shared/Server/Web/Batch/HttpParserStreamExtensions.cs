using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.IO;

namespace NuFridge.Shared.Server.Web.Batch
{
    public static class HttpParserStreamExtensions
    {
        public static Request ReadAsRequest(this Stream stream, IDictionary<string, IEnumerable<string>> headers)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var requestLine = RequestLineParser.Parse(stream);

            var url = new Url();

            if (requestLine.Uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                Uri uri;
                if (Uri.TryCreate(requestLine.Uri, UriKind.Absolute, out uri))
                {
                    url.Path = uri.AbsolutePath;
                    url.Query = uri.Query;
                    //url.Fragment = uri.Fragment;
                    url.Scheme = uri.Scheme;
                }
            }
            else
            {
                var splitUri = requestLine.Uri.Split('?');
                url.Path = splitUri[0];
                url.Query = splitUri.Length == 2 ? splitUri[1] : string.Empty;
                url.Scheme = "https";
            }


            IEnumerable<string> headerValues;
            if (headers.TryGetValue("Host", out headerValues))
            {
                url.HostName = headerValues.FirstOrDefault();
            }

            var nestedRequestStream = new RequestStream(new HttpMultipartSubStream(stream, stream.Position, stream.Length), stream.Length - stream.Position, true);

            var request = new Request(requestLine.Method, url, nestedRequestStream, headers, null);

            return request;
        }
    }
}