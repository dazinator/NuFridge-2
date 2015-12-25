using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using Dapper;
using NuFridge.Shared.Database.Model;
using Palmer;

namespace NuFridge.Shared.Database.Repository
{
    public class FeedGroupRepository : BaseRepository<FeedGroup>, IFeedGroupRepository
    {
        private readonly IFeedRepository _feedRepository;
        private const string TableName = "FeedGroup";
        private const string CacheKey = "FeedGroupModel-{0}";

        private string GetCacheKey(int groupId)
        {
            return string.Format(CacheKey, groupId);
        }

        public FeedGroupRepository(IFeedRepository feedRepository, DatabaseContext dbContext) : base(dbContext, TableName)
        {
            _feedRepository = feedRepository;
        }

        public void Insert(FeedGroup feedGroup)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        feedGroup.Id = connection.Insert<int>(feedGroup);
                    }
                });

            var cacheKey = GetCacheKey(feedGroup.Id);

            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(6) };
            MemoryCache.Default.Set(cacheKey, feedGroup, policy);
        }

        private List<Feed> GetGroupFeeds(int groupId)
        {
            return _feedRepository.FindByGroupId(groupId).ToList();
        }

        public FeedGroup Find(string feedGroupName)
        {
            FeedGroup group;

            using (var connection = GetConnection())
            {
                group =
                    connection.Query<FeedGroup>($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE Name = @name",
                        new { name = feedGroupName }).FirstOrDefault();
                if (group != null)
                {
                    group.Feeds = GetGroupFeeds(group.Id);
                }
            }

            if (group != null)
            {
                var cacheKey = GetCacheKey(group.Id);
                CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(6) };
                MemoryCache.Default.Set(cacheKey, group, policy);
            }

            return group;
        }

        public void Delete(FeedGroup feedGroup)
        {
            ThrowIfReadOnly();

            using (var connection = GetConnection())
            {
                connection.Delete(feedGroup);
            }
        }

        public void Update(FeedGroup feedGroup)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        connection.Update(feedGroup);
                    }
                });

            var cacheKey = GetCacheKey(feedGroup.Id);

            CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(6) };
            MemoryCache.Default.Set(cacheKey, feedGroup, policy);
        }

        public override FeedGroup Find(int id)
        {
            var cacheKey = GetCacheKey(id);

            var cachedRecord = MemoryCache.Default.Get(cacheKey);

            if (cachedRecord != null)
            {
                return (FeedGroup)cachedRecord;
            }

            FeedGroup group;

            using (var connection = GetConnection())
            {
                group = connection.Get<FeedGroup>(id);
                if (group != null)
                {
                    group.Feeds = GetGroupFeeds(group.Id);
                }
            }

            if (group != null)
            {
                CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(6) };
                MemoryCache.Default.Set(cacheKey, group, policy);
            }

            return group;
        }

        public override IEnumerable<FeedGroup> GetAll()
        {
            using (var connection = GetConnection())
            {
                IEnumerable<FeedGroup> groups = connection.GetList<FeedGroup>().ToList();
                foreach (var group in groups)
                {
                    group.Feeds = GetGroupFeeds(group.Id);
                }

                return groups;
            }
        }
    }

    public interface IFeedGroupRepository
    {
        IEnumerable<FeedGroup> GetAll();
        FeedGroup Find(int id);
        int GetCount(bool nolock);
        void Update(FeedGroup feedGroup);
        void Insert(FeedGroup feedGroup);
        FeedGroup Find(string name);
        void Delete(FeedGroup feedGroup);
    }
}