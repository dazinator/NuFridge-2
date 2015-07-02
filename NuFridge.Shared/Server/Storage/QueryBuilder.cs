using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuFridge.Shared.Server.Storage
{
    public class QueryBuilder<TRecord> : IQueryBuilder<TRecord> where TRecord : class
    {
        private readonly CommandParameters _parameters = new CommandParameters();
        private readonly StringBuilder _whereClauses = new StringBuilder();
        private string _orderBy = "Id";
        private readonly ITransaction _transaction;
        private string _viewOrTableName;
        private string _tableHint;
        private string distinctColumn;

        public QueryBuilder(ITransaction transaction, string viewOrTableName)
        {
            _transaction = transaction;
            _viewOrTableName = viewOrTableName;
        }

        public IQueryBuilder<TRecord> Distinct(string column)
        {
            if (!string.IsNullOrWhiteSpace(column))
            {
                distinctColumn = column;
            }

            return this;
        }

        public IQueryBuilder<TRecord> Where(string whereClause)
        {
            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                if (_whereClauses.Length > 0)
                    _whereClauses.Append(" AND ");
                else
                    _whereClauses.Append("WHERE ");
                _whereClauses.Append('(');
                _whereClauses.Append(whereClause);
                _whereClauses.Append(')');
            }
            return this;
        }

        public IQueryBuilder<TRecord> Parameter(string name, object value)
        {
            _parameters.Add(name, value);
            return this;
        }

        public IQueryBuilder<TRecord> LikeParameter(string name, object value)
        {
            _parameters.Add(name, "%" + (value ?? string.Empty).ToString().Replace("%", "[%]") + "%");
            return this;
        }

        public IQueryBuilder<TRecord> View(string viewName)
        {
            _viewOrTableName = viewName;
            return this;
        }

        public IQueryBuilder<TRecord> Table(string tableName)
        {
            _viewOrTableName = tableName;
            return this;
        }

        public IQueryBuilder<TRecord> Hint(string tableHintClause)
        {
            _tableHint = tableHintClause;
            return this;
        }

        public IQueryBuilder<TRecord> OrderBy(string orderByClause)
        {
            _orderBy = orderByClause;
            return this;
        }

        public int Count()
        {
            return _transaction.ExecuteScalar<int>("SELECT COUNT(*) FROM NuFridge.[" + _viewOrTableName + "] " + _tableHint + " " + _whereClauses, _parameters);
        }

        public TRecord First()
        {
            return _transaction.ExecuteReader<TRecord>("SELECT TOP 1 * FROM NuFridge.[" + _viewOrTableName + "] " + ToString(), _parameters).FirstOrDefault();
        }

        public List<TRecord> ToList(int skip, int take)
        {
            Parameter("_minrow", skip + 1);
            Parameter("_maxrow", take + skip);
            var select = distinctColumn != null ? "DISTINCT " + distinctColumn : "*";
            return _transaction.ExecuteReader<TRecord>("SELECT " + select + " FROM (SELECT *, Row_Number() over (ORDER BY " + _orderBy + ") as RowNum FROM NuFridge.[" + _viewOrTableName + "] " + _whereClauses + ") RS WHERE RowNum >= @_minrow And RowNum <= @_maxrow", _parameters).ToList();
        }

        public List<TRecord> ToList(int skip, int take, out int totalResults)
        {
            totalResults = Count();
            return ToList(skip, take);
        }

        public List<TRecord> ToList()
        {
            return Stream().ToList();
        }

        public IEnumerable<TRecord> Stream()
        {
            return _transaction.ExecuteReader<TRecord>("SELECT * FROM NuFridge.[" + _viewOrTableName + "] " + ToString(), _parameters);
        }

        public IEnumerable<TRecord> Stream(Func<IProjectionMapper, TRecord> projectionMapper)
        {
            return _transaction.ExecuteReaderWithProjection("SELECT * FROM NuFridge.[" + _viewOrTableName + "] " + ToString(), _parameters, projectionMapper);
        }

        public IDictionary<string, TRecord> ToDictionary(Func<TRecord, string> keySelector)
        {
            return Stream().ToDictionary(keySelector, StringComparer.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return string.Concat(_tableHint == null ?  "" :  (_tableHint + " "),  _whereClauses,  " ORDER BY ",  _orderBy);
        }
    }
}
