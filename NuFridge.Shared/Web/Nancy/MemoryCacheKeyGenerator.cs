using System;
using System.Text;
using Nancy;
using Nancy.LightningCache.CacheKey;

namespace NuFridge.Shared.Web.Nancy
{
    public class MemoryCacheKeyGenerator : ICacheKeyGenerator
    {
        public string Get(Request request)
        {
            using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(request.Url.ToString()));
                return Convert.ToBase64String(hash);
            }
        }
    }
}