using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Autofac;
using Newtonsoft.Json;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Storage
{
    public class Transaction : ITransaction, IDisposable
    {
        private static readonly ConcurrentDictionary<string, string> InsertStatementTemplates = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> UpdateStatementTemplates = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<Transaction, bool> CurrentTransactions = new ConcurrentDictionary<Transaction, bool>();
        private readonly List<string> _commands = new List<string>();
        private readonly object _sync = new object();
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly RelationalMappings _mappings;
        private readonly ILog _log = LogProvider.For<Transaction>();

        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;
        private readonly IContainer _container;

        public Transaction(IContainer container, string connectionString, IsolationLevel isolationLevel, JsonSerializerSettings jsonSerializerSettings, RelationalMappings mappings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _mappings = mappings;
            _connection = new SqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction(isolationLevel);
            CurrentTransactions.TryAdd(this, true);
            _container = container;
        }

        public T Load<T>(int id) where T : class
        {
            return Query<T>().Where("Id = @id").Parameter("id", id).First();
        }

        public T[] Load<T>(IEnumerable<int> ids) where T : class
        {
            return Query<T>().Where("Id in @ids").Parameter("ids", ids.ToArray()).Stream().ToArray();
        }

        public T LoadRequired<T>(int id) where T : class
        {
            T obj = Load<T>(id);
            if (obj == null)
                throw new ResourceNotFoundException(id.ToString());
            return obj;
        }

        public T[] LoadRequired<T>(IEnumerable<int> ids) where T : class
        {
            int[] strArray = ids.ToArray();
            T[] objArray = Query<T>().Where("Id in @ids").Parameter("ids", strArray).Stream().ToArray();
            foreach (Tuple<int, T> tuple in strArray.Zip<int, T, Tuple<int, T>>(objArray, Tuple.Create))
            {
                if (tuple.Item2 == null)
                    throw new ResourceNotFoundException(tuple.Item1.ToString());
            }
            return objArray;
        }

        public void Insert<TDocument>(TDocument instance) where TDocument : class
        {
            Insert(null, instance);
        }

        public void Insert<TDocument>(string tableName, TDocument instance) where TDocument : class
        {
            EntityMapping mapping;
            if (!_mappings.TryGet(instance.GetType(), out mapping))
            {
                mapping = _mappings.Get(typeof(TDocument));
            }

            string orAdd = InsertStatementTemplates.GetOrAdd(mapping.TableName, t => string.Format("INSERT INTO NuFridge.[{0}] ({1}) values ({2})", tableName ?? mapping.TableName, 
                string.Join(", ", mapping.IndexedColumns.Where(c => c.Writable).Select(c => c.ColumnName)), string.Join(", ", mapping.IndexedColumns.Where(c => c.Writable).Select(c => "@" + c.ColumnName))));

            CommandParameters args = InstanceToParameters(instance, mapping);

            using (SqlCommand command = CreateCommand(orAdd, args))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    _log.ErrorException(ex.Message, ex);

                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        UniqueRule uniqueRule = mapping.UniqueConstraints.FirstOrDefault(u => ex.Message.Contains(u.ConstraintName));
                        if (uniqueRule != null)
                            throw new UniqueConstraintViolationException(uniqueRule.Message);
                    }
                    throw WrapException(command, ex);
                }
            }
        }

        public void Update<TDocument>(TDocument instance) where TDocument : class
        {
            Update(null, instance);
        }

        public void Update<TDocument>(string tableName, TDocument instance) where TDocument : class
        {
            EntityMapping mapping;
            if (!_mappings.TryGet(instance.GetType(), out mapping))
            {
                mapping = _mappings.Get(typeof (TDocument));
            }
            var columns = mapping.IndexedColumns.Count > 0 ? string.Join(", ", mapping.IndexedColumns.Where(c => c.Writable).Select(c => "[" + c.ColumnName + "] = @" + c.ColumnName)): "";
            using (SqlCommand command = CreateCommand(UpdateStatementTemplates.GetOrAdd(mapping.TableName, t => string.Format("UPDATE NuFridge.[{0}] SET {1} WHERE Id = @Id", tableName ?? mapping.TableName, columns)), InstanceToParameters(instance, mapping)))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    _log.ErrorException(ex.Message, ex);

                    throw WrapException(command, ex);
                }
            }
        }

        public void Delete<TDocument>(TDocument instance) where TDocument : class
        {
            EntityMapping mapping;
            if (!_mappings.TryGet(instance.GetType(), out mapping))
            {
                mapping = _mappings.Get(typeof(TDocument));
            }
            string statement = string.Format("DELETE from NuFridge.[{0}] WHERE Id = @Id", mapping.TableName);
            CommandParameters commandParameters = new CommandParameters();

            object value;
            mapping.IdColumn.ReaderWriter.Read(instance, out value);

            commandParameters.Add("Id", value);
            CommandParameters args = commandParameters;
            using (SqlCommand command = CreateCommand(statement, args))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    _log.ErrorException(ex.Message, ex);

                    throw WrapException(command, ex);
                }
            }
        }

        public IEnumerable<T> ExecuteReader<T>(string query, CommandParameters args)
        {
            EntityMapping mapping = _mappings.Get(typeof(T));
            using (SqlCommand command = CreateCommand(query, args))
            {
                try
                {
                    return Stream<T>(command, mapping);
                }
                catch (SqlException ex)
                {
                    _log.ErrorException(ex.Message, ex);

                    throw WrapException(command, ex);
                }
            }
        }

        public IEnumerable<T> ExecuteReaderWithProjection<T>(string query, CommandParameters args, Func<IProjectionMapper, T> projectionMapper)
        {
            using (SqlCommand command = CreateCommand(query, args))
            {
                try
                {
                    return Stream(command, projectionMapper);
                }
                catch (SqlException ex)
                {
                    _log.ErrorException(ex.Message, ex);

                    throw WrapException(command, ex);
                }
            }
        }

        private IEnumerable<T> Stream<T>(SqlCommand command, EntityMapping mapping)
        {
            var type = typeof (T);

        
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                int idIndex = GetOrdinal(reader, "Id");

                Dictionary<ColumnMapping, int> columnIndexes = mapping.IndexedColumns.ToDictionary(c => c, c => GetOrdinal(reader, c.ColumnName));
                while (reader.Read())
                {
                    T instance;
                    if (type.IsInterface)
                    {
                        instance = _container.Resolve<T>();
                    }
                    else
                    {
                        instance = Activator.CreateInstance<T>();
                    }

                    foreach (KeyValuePair<ColumnMapping, int> keyValuePair in columnIndexes)
                    {
                        if (keyValuePair.Value >= 0)
                            keyValuePair.Key.ReaderWriter.Write(instance, reader[keyValuePair.Value]);
                    }
                    if (idIndex >= 0)
                        mapping.IdColumn.ReaderWriter.Write(instance, reader[idIndex]);
                    yield return instance;
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }
        }

        private static int GetOrdinal(SqlDataReader dr, string columnName)
        {
            for (int ordinal = 0; ordinal < dr.FieldCount; ++ordinal)
            {
                if (dr.GetName(ordinal).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return ordinal;
            }
            return -1;
        }

        private IEnumerable<T> Stream<T>(SqlCommand command, Func<IProjectionMapper, T> projectionMapper)
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                ProjectionMapper mapper = new ProjectionMapper(_container, reader, _jsonSerializerSettings, _mappings);
                while (reader.Read())
                    yield return projectionMapper(mapper);
            }
        }

        public IQueryBuilder<T> Query<T>() where T : class
        {
            return new QueryBuilder<T>(this, _mappings.Get(typeof(T)).TableName);
        }

        private CommandParameters InstanceToParameters(object instance, EntityMapping mapping)
        {
            CommandParameters commandParameters = new CommandParameters();

            object objId;
            mapping.IdColumn.ReaderWriter.Read(instance, out objId);
            commandParameters["Id"] = objId;
            foreach (ColumnMapping columnMapping in mapping.IndexedColumns)
            {
                object obj;
                if (columnMapping.ReaderWriter.Read(instance, out obj))
                {
                    if (obj != null && obj != DBNull.Value && (obj is string && columnMapping.MaxLength > 0))
                    {
                        int length = ((string) obj).Length;
                        if (length > columnMapping.MaxLength)
                            throw new StringTooLongException(
                                string.Format(
                                    "An attempt was made to store {0} characters in the {1}.{2} column, which only allows {3} characters.",
                                    length, mapping.TableName, columnMapping.ColumnName, columnMapping.MaxLength));
                    }
                    commandParameters[columnMapping.ColumnName] = obj;
                }
            }
            return commandParameters;
        }

        public T ExecuteScalar<T>(string query, CommandParameters args)
        {
            using (SqlCommand command = CreateCommand(query, args))
            {
                try
                {
                    return (T)Converter.Convert(command.ExecuteScalar(), typeof(T));
                }
                catch (SqlException ex)
                {
                    _log.ErrorException(ex.Message, ex);

                    throw WrapException(command, ex);
                }
            }
        }

        public void ExecuteReader(string query, Action<IDataReader> readerCallback)
        {
            ExecuteReader(query, null, readerCallback);
        }

        public void ExecuteReader(string query, object args, Action<IDataReader> readerCallback)
        {
            ExecuteReader(query, new CommandParameters(args), readerCallback);
        }

        public void ExecuteReader(string query, CommandParameters args, Action<IDataReader> readerCallback)
        {
            using (SqlCommand command = CreateCommand(query, args))
            {
                try
                {
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                        readerCallback(sqlDataReader);
                }
                catch (SqlException ex)
                {
                    _log.ErrorException(ex.Message, ex);

                    throw WrapException(command, ex);
                }
            }
        }

        private SqlCommand CreateCommand(string statement, CommandParameters args)
        {
            SqlCommand command = new SqlCommand(statement, _connection, _transaction);
            if (args != null)
                args.ContributeTo(command);
            lock (_sync)
                _commands.Add(command.CommandText);
            return command;
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
            bool flag;
            CurrentTransactions.TryRemove(this, out flag);
        }

        private Exception WrapException(SqlCommand command, Exception ex)
        {
            SqlException sqlException = ex as SqlException;
            if (sqlException != null && sqlException.Number == 1205)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(ex.Message);
                stringBuilder.AppendLine("Current transactions: ");
                int num = 0;
                foreach (KeyValuePair<Transaction, bool> keyValuePair in CurrentTransactions)
                {
                    stringBuilder.AppendLine("  Transaction " + num);
                    lock (keyValuePair.Key._sync)
                    {
                        foreach (string item0 in keyValuePair.Key._commands)
                            stringBuilder.AppendLine("    " + item0);
                    }
                    ++num;
                }
                throw new Exception(stringBuilder.ToString());
            }
            return new Exception("Error while executing SQL command: " + ex.Message + Environment.NewLine + "The command being executed was:" + Environment.NewLine + command.CommandText + Environment.NewLine + " with parameters: " + string.Join(", ", command.Parameters.OfType<SqlParameter>().Select(p => "@" + p.ParameterName + " = '" + p.Value + "'")), ex);
        }

        private class ProjectionMapper : IProjectionMapper
        {
            private readonly SqlDataReader _reader;
            private readonly JsonSerializerSettings _jsonSerializerSettings;
            private readonly RelationalMappings _mappings;
            private readonly IContainer _container;

            public ProjectionMapper(IContainer container, SqlDataReader reader, JsonSerializerSettings jsonSerializerSettings, RelationalMappings mappings)
            {
                _container = container;
                _mappings = mappings;
                _reader = reader;
                _jsonSerializerSettings = jsonSerializerSettings;
            }

            public TResult Map<TResult>(string prefix)
            {
                EntityMapping documentMap = _mappings.Get(typeof(TResult));

                var type = typeof (TResult);

                TResult result;
                if (type.IsInterface)
                {
                    result = _container.Resolve<TResult>();
                }
                else
                {
                    result = Activator.CreateInstance<TResult>();
                }

                foreach (ColumnMapping columnMapping in documentMap.IndexedColumns)
                    columnMapping.ReaderWriter.Write(result, _reader[GetColumnName(prefix, columnMapping.ColumnName)]);
                documentMap.IdColumn.ReaderWriter.Write(result, _reader[GetColumnName(prefix, documentMap.IdColumn.ColumnName)]);
                return result;
            }

            public void Read(Action<IDataReader> callback)
            {
                callback(_reader);
            }

            private string GetColumnName(string prefix, string name)
            {
                if (!string.IsNullOrWhiteSpace(prefix))
                    return prefix + "_" + name;
                return name;
            }
        }
    }
}
