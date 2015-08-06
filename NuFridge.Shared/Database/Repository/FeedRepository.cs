using System.Collections.Generic;
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

        public IEnumerable<Feed> Search(string name)
        {
            return Query($"SELECT * FROM [NuFridge].[{TableName}] WHERE Name LIKE '%' + @name + '%'", new {name});
        }
    }

    public interface IFeedRepository
    {
        void Insert(Feed feed);
        IEnumerable<Feed> GetAll();
        Feed Find(int feedId);
        void Delete(Feed feed);
        IEnumerable<Feed> Search(string name);
        IEnumerable<Feed> GetAllPaged(int pageNumber, int rowsPerPage);
        int GetCount();
    }
}