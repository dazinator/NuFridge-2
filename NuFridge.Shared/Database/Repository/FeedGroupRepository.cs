using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

        public FeedGroupRepository(IFeedRepository feedRepository) : base(TableName)
        {
            _feedRepository = feedRepository;
        }

        public void Insert(FeedGroup feedGroup)
        {
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
        }

        public void Update(FeedGroup feedGroup)
        {
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
        }

        public override FeedGroup Find(int id)
        {
            using (var connection = GetConnection())
            {
                FeedGroup group = connection.Get<FeedGroup>(id);
                if (group != null)
                {
                    group.Feeds = _feedRepository.FindByGroupId(id);
                }

                return group;
            }
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
        int GetCount();
        void Update(FeedGroup feedGroup);
        void Insert(FeedGroup feedGroup);
    }
}