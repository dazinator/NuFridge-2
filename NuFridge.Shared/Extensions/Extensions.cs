using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Nancy;
using Nancy.Cookies;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Model.Mappings;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Extensions
{
    public static class Extensions
    {
        public static string UrlEncode(this string input)
        {
            return Regex.Replace(Uri.EscapeDataString(input).Replace("%20", "+"), "(%[0-9A-F][0-9A-F])", c => c.Value.ToLower());
        }

        public static string UrlDecode(this string input)
        {
            return Uri.UnescapeDataString(input.Replace("+", "%20"));
        }

        public static TResponse WithCookies<TResponse>(this TResponse response, Response source) where TResponse : Response
        {
            if (source != null && source.Cookies != null)
            {
                using (IEnumerator<INancyCookie> enumerator = source.Cookies.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        INancyCookie current = enumerator.Current;
                        response.WithCookie(current);
                    }
                }
            }
            return response;
        }

        public static Response AsBytes(this IResponseFormatter responseFormatter, Func<Stream> getContentStream, string contentType)
        {
            Response response = new Response
            {
                ContentType = (contentType),
                Contents = output =>
                {
                    using (Stream stream = getContentStream())
                        stream.CopyTo(output);
                }
            };
            return response;
        }
    }
}
