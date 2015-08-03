using System;
using System.Data;
using System.Reflection;

namespace NuFridge.Shared.Server.Storage
{
    public class ColumnMapping
    {
        private DbType? _dbType;
        private int _maxLength;

        public bool Writable { get; set; }
        public bool IsNullable { get; set; }

        public string ColumnName { get; private set; }

        public DbType DbType
        {
            get
            {
                if (!_dbType.HasValue)
                    return DbType = DatabaseTypeConverter.AsDbType(Property.PropertyType);
                return _dbType.Value;
            }
            set
            {
                _dbType = value;
            }
        }

        public int MaxLength
        {
            get
            {
                if (_maxLength == 0)
                {
                    DbType? nullable = _dbType;
                    if ((nullable.GetValueOrDefault() != DbType.String ? 0 : (nullable.HasValue ? 1 : 0)) != 0)
                        MaxLength = 100;
                }
                return _maxLength;
            }
            set
            {
                _maxLength = value;
            }
        }

        public PropertyInfo Property { get; private set; }

        public IPropertyReaderWriter<object> ReaderWriter { get; set; }

        public ColumnMapping(string columnName, DbType dbType, IPropertyReaderWriter<object> readerWriter)
        {
            if (columnName == null)
                throw new ArgumentNullException("columnName");
            if (readerWriter == null)
                throw new ArgumentNullException("readerWriter");
            _dbType = dbType;
            ColumnName = columnName;
            ReaderWriter = readerWriter;
            Writable = true;
        }

        public ColumnMapping(PropertyInfo property)
        {
            Writable = true;
            Property = property;
            ColumnName = Property.Name;
            ReaderWriter = PropertyReaderFactory.Create<object>(property.DeclaringType, property.Name);
            if (property.PropertyType.IsGenericType && typeof(Nullable<>).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition()))
                IsNullable = true;
            if (property.PropertyType.IsEnum)
            {
                MaxLength = 50;
                DbType = DbType.String;
            }
            if (!(property.PropertyType == typeof(ReferenceCollection)))
                return;
            DbType = DbType.String;
            MaxLength = int.MaxValue;
            ReaderWriter = new ReferenceCollectionReaderWriter(ReaderWriter);
        }

        public ColumnMapping Nullable()
        {
            IsNullable = true;
            return this;
        }

        public ColumnMapping WithMaxLength(int max)
        {
            _maxLength = max;
            return this;
        }
    }
}
