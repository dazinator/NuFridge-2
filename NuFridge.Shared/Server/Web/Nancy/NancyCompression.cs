using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nancy;

namespace NuFridge.Shared.Server.Web.Nancy
{
    public static class NancyCompression
    {
        private static readonly HashSet<string> ValidMimes = new HashSet<string>
        {
      "text/css",
      "text/javascript",
      "text/html",
      "text/plain",
      "application/xml",
      "application/json",
      "application/xaml+xml",
      "application/x-javascript",
      "application/javascript"
    };

        public static Func<NancyContext, string, Response> CompressStaticContent(Func<NancyContext, string, Response> responsomatic)
        {
            return (context, s) =>
            {
                Response response = responsomatic(context, s);
                if (response == null)
                    return (Response)null;
                response.Headers["Vary"] = "Accept-Encoding";
                if (!RequestIsGzipCompatible(context.Request))
                    return response;
                response.Headers["Expires"] = DateTimeOffset.UtcNow.AddHours(1.0).ToString("R", DateTimeFormatInfo.InvariantInfo);
                int num = (int)response.StatusCode;
                if (num < 200 || num > 300 || (!ResponseIsCompatibleMimeType(response) || ContentLengthIsTooSmall(response)))
                    return response;
                CompressResponse(response);
                return response;
            };
        }

        public static void CompressIfPossible(NancyContext context)
        {
            if (!RequestIsGzipCompatible(context.Request) || (int)context.Response.StatusCode != 200 || (!ResponseIsCompatibleMimeType(context.Response) || ContentLengthIsTooSmall(context.Response)))
                return;
            CompressResponse(context.Response);
        }

        private static void CompressResponse(Response response)
        {
            response.Headers["Content-Encoding"] = "gzip";
            Action<Stream> contents = response.Contents;
            response.Contents = responseStream =>
            {
                using (GZipStream gzipStream = new GZipStream(responseStream, CompressionMode.Compress))
                    contents(gzipStream);
            };
        }

        private static bool ContentLengthIsTooSmall(Response response)
        {
            string s;
            return response.Headers.TryGetValue("Content-Length", out s) && long.Parse(s) < 4096L;
        }

        private static bool ResponseIsCompatibleMimeType(Response response)
        {
            if (string.IsNullOrWhiteSpace(response.ContentType))
                return false;
            if (!ValidMimes.Contains(response.ContentType))
                return ValidMimes.Any(m => response.ContentType.StartsWith(m + ";"));
            return true;
        }

        private static bool RequestIsGzipCompatible(Request request)
        {
            return request.Headers.AcceptEncoding.Any(x => x.Contains("gzip"));
        }
    }
}
