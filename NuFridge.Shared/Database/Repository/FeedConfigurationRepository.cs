using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using Dapper;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server;
using Palmer;

namespace NuFridge.Shared.Database.Repository
{
    public class FeedConfigurationRepository : BaseRepository<FeedConfiguration>, IFeedConfigurationRepository
    {
        private const string TableName = "FeedConfiguration";
        private const string CacheKey = "FeedConfigurationModel-{0}";

        private string GetCacheKey(int feedId)
        {
            return string.Format(CacheKey, feedId);
        }

        public FeedConfigurationRepository() : base(TableName)
        {
        }

        public void Insert(FeedConfiguration feedConfiguration)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        feedConfiguration.Id = connection.Insert<int>(feedConfiguration);
                    }
                });

            var cacheKey = GetCacheKey(feedConfiguration.FeedId);

            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(1) };
            MemoryCache.Default.Set(cacheKey, feedConfiguration, policy);
        }

        public override void Delete(FeedConfiguration feedConfiguration)
        {
            ThrowIfReadOnly();

            var cacheKey = GetCacheKey(feedConfiguration.FeedId);

            MemoryCache.Default.Remove(cacheKey);

            base.Delete(feedConfiguration);
        }

        public FeedConfiguration FindByFeedId(int feedId)
        {
            var cacheKey = GetCacheKey(feedId);

            var cachedFeedConfig = MemoryCache.Default.Get(cacheKey);

            if (cachedFeedConfig != null)
            {
                return (FeedConfiguration)cachedFeedConfig;
            }

            var feedConfig = Query<FeedConfiguration>($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE FeedId = @feedId", new { feedId }).SingleOrDefault();

            if (feedConfig != null)
            {
                CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(1) };
                MemoryCache.Default.Set(cacheKey, feedConfig, policy);
            }

            return feedConfig;
        }

        public void Update(FeedConfiguration feedConfiguration)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        connection.Update(feedConfiguration);
                    }
                });

            var cacheKey = GetCacheKey(feedConfiguration.FeedId);

            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(1) };
            MemoryCache.Default.Set(cacheKey, feedConfiguration, policy);
        }
    }

    public interface IFeedConfigurationRepository
    {
        void Insert(FeedConfiguration feedConfiguration);
        void Delete(FeedConfiguration feedConfiguration);
        FeedConfiguration FindByFeedId(int feedId);
        void Update(FeedConfiguration feedConfig);
        IEnumerable<FeedConfiguration> GetAll();
    }
}