using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace NuFridge.Shared.Server.Storage
{
    public abstract class EntityMapping
    {
        public string TableName { get; protected set; }

        public string IdPrefix { get; protected set; }

        public bool IsProjection { get; protected set; }

        public Type Type { get; protected set; }

        public ColumnMapping IdColumn { get; protected set; }

        public List<ColumnMapping> IndexedColumns { get; private set; }

        public List<UniqueRule> UniqueConstraints { get; private set; }

        protected EntityMapping()
        {
            IndexedColumns = new List<ColumnMapping>();
            UniqueConstraints = new List<UniqueRule>();
        }

        protected void InitializeDefault(Type type)
        {
            Type = type;
            TableName = type.Name;
            IdPrefix = TableName + "s";
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.Name == "Id")
                    IdColumn = new ColumnMapping(property);
            }
        }
    }
    public abstract class EntityMapping<TDocument> : EntityMapping
    {
        protected EntityMapping()
        {
            InitializeDefault(typeof(TDocument));
        }

        protected ColumnMapping Column<T>(Expression<Func<TDocument, T>> property)
        {
            ColumnMapping columnMapping = new ColumnMapping(GetPropertyInfo(property));
            IndexedColumns.Add(columnMapping);
            return columnMapping;
        }

        protected ColumnMapping Column<T>(Expression<Func<TDocument, T>> property, Action<ColumnMapping> configure)
        {
            ColumnMapping columnMapping = Column(property);
            configure(columnMapping);
            return columnMapping;
        }

        protected ColumnMapping VirtualColumn<TProperty>(string name, DbType databaseType, Func<TDocument, TProperty> reader, Action<TDocument, TProperty> writer = null, int? maxLength = null, bool nullable = false, bool writable = true)
        {
            ColumnMapping columnMapping = new ColumnMapping(name, databaseType, new DelegateReaderWriter<TDocument, TProperty>(reader, writer));
            IndexedColumns.Add(columnMapping);
            if (maxLength.HasValue)
                columnMapping.MaxLength = maxLength.Value;
            columnMapping.IsNullable = nullable;
            columnMapping.Writable = writable;
            return columnMapping;
        }

        private static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            MemberExpression memberExpression = propertyLambda.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", propertyLambda));
            PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", propertyLambda));
            return propertyInfo;
        }

        protected UniqueRule Unique(string constraintName, string columnName, string errorMessage)
        {
            UniqueRule uniqueRule = new UniqueRule(constraintName, columnName)
            {
                Message = errorMessage
            };
            UniqueConstraints.Add(uniqueRule);
            return uniqueRule;
        }

        protected UniqueRule Unique(string constraintName, string[] columnNames, string errorMessage)
        {
            UniqueRule uniqueRule = new UniqueRule(constraintName, columnNames)
            {
                Message = errorMessage
            };
            UniqueConstraints.Add(uniqueRule);
            return uniqueRule;
        }
    }
}
