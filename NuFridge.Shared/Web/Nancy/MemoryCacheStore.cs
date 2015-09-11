using System;
using System.Runtime.Caching;
using Nancy;
using Nancy.LightningCache.CacheStore;
using Nancy.LightningCache.Projection;

namespace NuFridge.Shared.Web.Nancy
{
    public class MemoryCacheStore : ICacheStore
    {
        private static readonly object Lock = new object();
        private MemoryCache _cache;

        public CachedResponse Get(string key)
        {
            SetCache();
            SerializableResponse response = _cache?.Get(key) as SerializableResponse;
            if (response == null)
                return null;
            return new CachedResponse(response);
        }

        public void Remove(string key)
        {
            SetCache();
            _cache?.Remove(key);
        }

        public void Set(string key, NancyContext context, DateTime absoluteExpiration)
        {
            SetCache();
            if (_cache == null)
                return;
            _cache[key] = new SerializableResponse(context.Response, absoluteExpiration);
        }

        private void SetCache()
        {
            lock (Lock)
            {
                _cache = MemoryCache.Default;
            }
        }
    }
}