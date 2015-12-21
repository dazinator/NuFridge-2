using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using NuFridge.Shared.Database.Model;
using Palmer;

namespace NuFridge.Shared.Database.Repository
{
    public class FrameworkRepository : BaseRepository<Framework>, IFrameworkRepository
    {
        private const string TableName = "Framework";

        public FrameworkRepository(DatabaseContext dbContext) : base(dbContext, TableName)
        {

        }

        public void Insert(Framework framework)
        {
            ThrowIfReadOnly();

            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        framework.Id = connection.Insert<int>(framework);
                    }
                });
        }
    }

    public interface IFrameworkRepository
    {
        IEnumerable<Framework> GetAll();
        void Insert(Framework framework);
    }
}