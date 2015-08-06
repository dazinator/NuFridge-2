using System.Linq;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class FeedConfigurationRepository : BaseRepository<FeedConfiguration>, IFeedConfigurationRepository
    {
        private const string TableName = "FeedConfiguration";

        public FeedConfigurationRepository() : base(TableName)
        {
            
        }

        public void Insert(FeedConfiguration feedConfiguration)
        {
            using (var connection = GetConnection())
            {
                feedConfiguration.Id = connection.Insert<int>(feedConfiguration);
            }
        }

        public FeedConfiguration FindByFeedId(int feedId)
        {
            return Query($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE FeedId = @feedId").SingleOrDefault();
        }
    }

    public interface IFeedConfigurationRepository
    {
        void Insert(FeedConfiguration feedConfiguration);
        void Delete(FeedConfiguration feedConfiguration);
        FeedConfiguration FindByFeedId(int feedId);
    }
}