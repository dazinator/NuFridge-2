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
    public class FeedGroupRepository : BaseRepository<FeedGroup>, IFeedGroupRepository
    {
        private readonly IFeedRepository _feedRepository;
        private const string TableName = "FeedGroup";
        private const string CacheKey = "FeedGroupModel-{0}";

        private string GetCacheKey(int groupId)
        {
            return string.Format(CacheKey, groupId);
        }

        public FeedGroupRepository(IFeedRepository feedRepository) : base(TableName)
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
                return (FeedGroup) cachedRecord;
            }

            FeedGroup group;

            using (var connection = GetConnection())
            {
                group = connection.Get<FeedGroup>(id);
                if (group != null)
                {
                    group.Feeds = _feedRepository.FindByGroupId(id);
                }
            }

            if (group != null)
            {
                CacheItemPolicy policy = new CacheItemPolicy {SlidingExpiration = TimeSpan.FromHours(6)};
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
                    group.Feeds = _feedRepository.FindByGroupId(group.Id);
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
    }
}