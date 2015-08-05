using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }

    public interface IFeedConfigurationRepository
    {
        void Insert(FeedConfiguration feedConfiguration);
    }
}