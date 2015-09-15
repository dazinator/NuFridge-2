using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class JobRepository : BaseRepository<Job>, IJobRepository
    {
        internal const string TableName = "Job";

        public JobRepository() : base(TableName)
        {
            
        }

        public void Insert(Job job)
        {
            ThrowIfReadOnly();

            using (var connection = GetConnection())
            {
                var query =
                    $"INSERT INTO [NuFridge].[{TableName}](Id, Name, FeedId, CreatedAt, CompletedAt, RetryCount, Success, HasWarnings, UserId) VALUES(@Id, @Name, @FeedId, @CreatedAt, @CompletedAt, @RetryCount, @Success, @HasWarnings, @UserId)";
                connection.Execute(query, job);
            }
        }

        public void Update(Job job)
        {
            ThrowIfReadOnly();

            using (var connection = GetConnection())
            {
                connection.Update(job);
            }
        }

        public IEnumerable<Job> Find(int pageNumber, int rowsPerPage, out int totalResults)
        {
            return GetAllPaged(pageNumber, rowsPerPage, out totalResults, null, "CreatedAt DESC", true);
        }

        public IEnumerable<Job> FindForFeed(int feedId, int pageNumber, int rowsPerPage, out int totalResults)
        {
            return GetAllPaged(pageNumber, rowsPerPage, out totalResults, "WHERE [FeedId] = " + feedId, "CreatedAt DESC", true);
        }

        public IEnumerable<Job> FindForFeed(int feedId)
        {
            return Query<Job>($"SELECT * FROM [NuFridge].[{TableName}] WITH(NOLOCK) WHERE [FeedId] = @feedId ORDER BY CreatedAt DESC",
                new {feedId});
        }
    }

    public interface IJobRepository
    {
        void Insert(Job job);
        void Update(Job job);
        IEnumerable<Job> Find(int pageNumber, int rowsPerPage, out int totalResults);
        Job Find(int jobId);
        IEnumerable<Job> FindForFeed(int feedId);
        IEnumerable<Job> FindForFeed(int feedId, int pageNumber, int rowsPerPage, out int totalResults);
    }
}