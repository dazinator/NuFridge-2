using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageImportJobRepository : BaseRepository<PackageImportJob>, IJobTypeRepository<IJobType>
    {
        private const string TableName = "Job_PackageImport";

        public PackageImportJobRepository() : base(TableName)
        {
        }

        public bool IsTypeCompatible<TRecord>() where TRecord : class, new()
        {
            return typeof(TRecord) == typeof(PackageImportJob);
        }

        public void Insert(IJobType job)
        {
            ThrowIfReadOnly();

            using (var connection = GetConnection())
            {
                var query =
                    $"INSERT INTO [NuFridge].[{TableName}](JobId, Processed, Scheduled, JSON) VALUES(@JobId, @Processed, @Scheduled, @JSON)";
                connection.Execute(query, job);
            }
        }

        public void Update(IJobType job)
        {
            ThrowIfReadOnly();

            using (var connection = GetConnection())
            {
                connection.Update(job);
            }
        }

        public TRecord Find<TRecord>(int jobId) where TRecord : class, IJobType, new()
        {
            using (var connection = GetConnection())
            {
                return connection.Get<TRecord>(jobId);
            }
        }

        public IEnumerable<TRecord> FindForFeed<TRecord>(int feedId) where TRecord : class, IJobType, new()
        {
            return Query<TRecord>($"SELECT TOP(50) * FROM [NuFridge].[{TableName}] WITH(NOLOCK) as pij INNER JOIN [NuFridge].[{JobRepository.TableName}] as j WITH(NOLOCK) on j.Id = pij.JobId WHERE j.FeedId = @feedId");
        }
    }
}