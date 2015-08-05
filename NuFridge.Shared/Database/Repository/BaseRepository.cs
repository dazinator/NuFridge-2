using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public abstract class BaseRepository<T> where T : class
    {
        private readonly string _tableName;
        private readonly string _connectionString;

        protected BaseRepository(string tableName)
        {
            _tableName = tableName;
            _connectionString = new SqlConnectionStringBuilder(DatabaseContext.ConnectionString.Value) { AsynchronousProcessing = true }.ToString();
        }

        public T Find(int id)
        {
            using (var connection = GetConnection())
            {
                return connection.Get<T>(id);
            }
        }

        public IEnumerable<T> GetAll()
        {
            using (var connection = GetConnection())
            {
                return connection.GetList<T>();
            }
        }

        public int GetCount()
        {
            using (var connection = GetConnection())
            {
                return connection.Query<int>($"SELECT COUNT(Id) FROM [NuFridge].[{_tableName}]").Single();
            }
        }

        protected IDbConnection GetConnection()
        {
            var connection = new SqlConnection(_connectionString);

            connection.Open();

            return connection;
        }
    }
}