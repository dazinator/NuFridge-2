using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageImportJobItemRepository : BaseRepository<PackageImportJobItem>, IPackageImportJobItemRepository
    {
        private const string TableName = "Job_PackageImportItem";

        public PackageImportJobItemRepository(DatabaseContext dbContext) : base(dbContext, TableName)
        {
            
        }

        public PackageImportJobItem Insert(PackageImportJobItem item)
        {
            ThrowIfReadOnly();

            using (var connection = GetConnection())
            {
                item.Id = connection.Insert<int>(item);
            }

            return item;
        }

        public void Update(PackageImportJobItem item)
        {
            ThrowIfReadOnly();

            using (var connection = GetConnection())
            {
                connection.Update(item);
            }
        }

        public IEnumerable<PackageImportJobItem> FindForJob(int jobId)
        {
            return Query<PackageImportJobItem>($"SELECT * FROM [NuFridge].[{TableName}] WITH(NOLOCK) WHERE JobId = @jobId", new {jobId});
        }
    }

    public interface IPackageImportJobItemRepository
    {
        PackageImportJobItem Insert(PackageImportJobItem item);
        void Update(PackageImportJobItem item);
        IEnumerable<PackageImportJobItem> FindForJob(int jobId);
    }
}