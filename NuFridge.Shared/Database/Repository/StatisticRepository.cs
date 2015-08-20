using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server;
using Palmer;

namespace NuFridge.Shared.Database.Repository
{
    public class StatisticRepository : BaseRepository<Statistic>, IStatisticRepository
    {
        private const string TableName = "Statistic";

        public StatisticRepository() : base(TableName)
        {
            
        }

        public Statistic Find(string statName)
        {
            using (var connection = GetConnection())
            {
                return connection.Query<Statistic>($"SELECT TOP(1) * FROM [NuFridge].[{TableName}] WHERE Name = @name", new { name = statName }).FirstOrDefault();
            }
        }


        public void Insert(Statistic statistic)
        {
            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        statistic.Id = connection.Insert<int>(statistic);
                    }
                });
        }

        public void Update(Statistic statistic)
        {
            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        connection.Update(statistic);
                    }
                });
        }
    }
    public interface IStatisticRepository
    {
        Statistic Find(string statName);
        void Insert(Statistic record);
        void Update(Statistic record);
    }
}