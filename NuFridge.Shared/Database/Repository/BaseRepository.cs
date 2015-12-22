using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database.Model;
using ReadOnlyException = NuFridge.Shared.Exceptions.ReadOnlyException;

namespace NuFridge.Shared.Database.Repository
{
    public abstract class BaseRepository<T> where T : class
    {
        private readonly string _tableName;
        private readonly string _connectionString;
        protected bool ReadOnly { get; set; }

        protected BaseRepository(DatabaseContext dbContext, string tableName)
        {
            _tableName = tableName;
            _connectionString = new SqlConnectionStringBuilder(dbContext.GetConnectionString()) { AsynchronousProcessing = true }.ToString();
            ReadOnly = dbContext.GetReadOnly();
        }

        protected void ThrowIfReadOnly()
        {
            if (ReadOnly)
            {
                throw new ReadOnlyException();
            }
        }



        public int RecordCount(bool noLock, string conditions = "")
        {
            Type type = typeof(T);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SELECT COUNT(1)");
            stringBuilder.AppendFormat(" FROM [NuFridge].[{0}]", _tableName);

            if (noLock)
            {
                stringBuilder.Append(" WITH(NOLOCK)");
            }

            stringBuilder.Append(" " + conditions);
            if (Debugger.IsAttached)
                Trace.WriteLine($"RecordCount<{(object)type}>: {(object)stringBuilder}");
            using (var connection = GetConnection())
            {
                return
                    connection.Query<int>(stringBuilder.ToString(), (object)null, (IDbTransaction)null, true,
                        new int?(), new CommandType?()).Single<int>();
            }
        }

        protected virtual void Delete(IEnumerable<int> ids, string idColumnName)
        {
            ThrowIfReadOnly();

            string listOfIdsJoined = "(" + String.Join(",", ids.ToArray()) + ")";

            using (var connection = GetConnection())
            {
                connection.Execute($"DELETE FROM [NuFridge].[{_tableName}] WHERE {idColumnName} IN {listOfIdsJoined}");
            }
        }

        public virtual IEnumerable<TRecord> Query<TRecord>(string sql, dynamic param = null)
        {
            using (var connection = GetConnection())
            {
                return SqlMapper.Query<TRecord>(connection, sql, param);
            }
        }

        public virtual IEnumerable<TRecord> Query<TRecord>(object whereConditions)
        {
            using (var connection = GetConnection())
            {
                return connection.GetList<TRecord>();
            }
        }

        public virtual T Find(int id)
        {
            using (var connection = GetConnection())
            {
                return connection.Get<T>(id);
            }
        }

        public virtual IEnumerable<T> GetAllPaged(int pageNumber, int rowsPerPage, out int totalCount, string conditions = null, string orderBy = null, bool nolock = false)
        {
            using (var connection = GetConnection())
            {
                totalCount = RecordCount(nolock, conditions);
                return connection.GetListPaged<T>(pageNumber, rowsPerPage, conditions, orderBy, null, null, nolock);
            }
        }

        public virtual IEnumerable<T> GetAll()
        {
            using (var connection = GetConnection())
            {
                return connection.GetList<T>();
            }
        }

        public virtual int GetCount(bool nolock)
        {
            using (var connection = GetConnection())
            {
                string query = $"SELECT COUNT(*) FROM [NuFridge].[{_tableName}]";
                if (nolock)
                {
                    query += " WITH(NOLOCK)";
                }
                return connection.Query<int>(query).Single();
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