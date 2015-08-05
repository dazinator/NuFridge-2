using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class FeedRepository : BaseRepository<Feed>, IFeedRepository
    {
        private const string TableName = "Feed";

        public FeedRepository() : base(TableName)
        {

        }

        public void Insert(Feed feed)
        {
            using (var connection = GetConnection())
            {
                feed.Id = connection.Insert<int>(feed);
            }
        }
    }

    public interface IFeedRepository
    {
        void Insert(Feed feed);
        IEnumerable<Feed> GetAll();
    }
}