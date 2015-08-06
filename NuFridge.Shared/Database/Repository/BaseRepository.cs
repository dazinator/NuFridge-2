using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

        public void Delete(T entity)
        {
            using (var connection = GetConnection())
            {
                connection.Delete(entity);
            }
        }

        protected void Delete(IEnumerable<int> ids, string idColumnName)
        {
            string listOfIdsJoined = "(" + String.Join(",", ids.ToArray()) + ")";

            using (var connection = GetConnection())
            {
                connection.Execute(string.Format("DELETE FROM [NuFridge].[{0}] WHERE {1} IN {2}", _tableName, idColumnName, listOfIdsJoined));
            }
        }

        public IEnumerable<T> Query(string sql, dynamic param = null)
        {
            using (var connection = GetConnection())
            {
                return SqlMapper.Query<T>(connection, sql, param);
            }
        }

        public T Find(int id)
        {
            using (var connection = GetConnection())
            {
                return connection.Get<T>(id);
            }
        }

        public IEnumerable<T> GetAllPaged(int pageNumber, int rowsPerPage)
        {
            using (var connection = GetConnection())
            {
                return connection.GetListPaged<T>(pageNumber, rowsPerPage, null, null);
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