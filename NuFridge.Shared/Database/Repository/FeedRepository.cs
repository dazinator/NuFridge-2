using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class FeedRepository : BaseRepository<Feed>, IFeedRepository
    {
        private const string TableName = "Feed";
        private const string CacheKey = "FeedModel-{0}";

        private string GetCacheKey(string feedName)
        {
            return string.Format(CacheKey, feedName);
        }

        public FeedRepository() : base(TableName)
        {

        }

        public override void Delete(Feed feed)
        {
            var cacheKey = GetCacheKey(feed.Name);

            MemoryCache.Default.Remove(cacheKey);

            base.Delete(feed);
        }

        public void Insert(Feed feed)
        {
            using (var connection = GetConnection())
            {
                feed.Id = connection.Insert<int>(feed);
            }

            var cacheKey = GetCacheKey(feed.Name);

            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(1) };
            MemoryCache.Default.Set(cacheKey, feed, policy);
        }

        public Feed Find(string feedName)
        {
            var cacheKey = GetCacheKey(feedName);

            var cachedFeedConfig = MemoryCache.Default.Get(cacheKey);

            if (cachedFeedConfig != null)
            {
                return (Feed) cachedFeedConfig;
            }

            Feed feed;
            using (var connection = GetConnection())
            {
                feed =
                    connection.Query<Feed>($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE Name = @name",
                        new {name = feedName}).FirstOrDefault();
            }

            if (feed != null)
            {
                CacheItemPolicy policy = new CacheItemPolicy {SlidingExpiration = TimeSpan.FromHours(1)};
                MemoryCache.Default.Set(cacheKey, feed, policy);
            }

            return feed;
        }

        public void Update(Feed feed)
        {
            using (var connection = GetConnection())
            {
                connection.Update(feed);
            }

            var cacheKey = GetCacheKey(feed.Name);

            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(1) };
            MemoryCache.Default.Set(cacheKey, feed, policy);
        }

        public IEnumerable<Feed> Search(string name)
        {
            return Query<Feed>($"SELECT * FROM [NuFridge].[{TableName}] WHERE Name LIKE '%' + @name + '%'", new {name});
        }
    }

    public interface IFeedRepository
    {
        void Insert(Feed feed);
        IEnumerable<Feed> GetAll();
        Feed Find(int feedId);
        Feed Find(string feedName);
        void Delete(Feed feed);
        IEnumerable<Feed> Search(string name);
        IEnumerable<Feed> GetAllPaged(int pageNumber, int rowsPerPage);
        int GetCount();
        void Update(Feed feed);
    }
}