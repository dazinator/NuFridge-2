using System;
using System.Collections.Generic;
using System.Data;

namespace NuFridge.Shared.Server.Storage
{
    public static class DatabaseTypeConverter
    {
        private static readonly Dictionary<Type, DbType> TypeMap = new Dictionary<Type, DbType>();

        static DatabaseTypeConverter()
        {
            TypeMap[typeof(byte)] = DbType.Byte;
            TypeMap[typeof(sbyte)] = DbType.SByte;
            TypeMap[typeof(short)] = DbType.Int16;
            TypeMap[typeof(ushort)] = DbType.UInt16;
            TypeMap[typeof(int)] = DbType.Int32;
            TypeMap[typeof(uint)] = DbType.UInt32;
            TypeMap[typeof(long)] = DbType.Int64;
            TypeMap[typeof(ulong)] = DbType.UInt64;
            TypeMap[typeof(float)] = DbType.Single;
            TypeMap[typeof(double)] = DbType.Double;
            TypeMap[typeof(Decimal)] = DbType.Decimal;
            TypeMap[typeof(bool)] = DbType.Boolean;
            TypeMap[typeof(string)] = DbType.String;
            TypeMap[typeof(char)] = DbType.StringFixedLength;
            TypeMap[typeof(Guid)] = DbType.Guid;
            TypeMap[typeof(DateTime)] = DbType.DateTime;
            TypeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            TypeMap[typeof(TimeSpan)] = DbType.Time;
            TypeMap[typeof(byte[])] = DbType.Binary;
            TypeMap[typeof(byte?)] = DbType.Byte;
            TypeMap[typeof(sbyte?)] = DbType.SByte;
            TypeMap[typeof(short?)] = DbType.Int16;
            TypeMap[typeof(ushort?)] = DbType.UInt16;
            TypeMap[typeof(int?)] = DbType.Int32;
            TypeMap[typeof(uint?)] = DbType.UInt32;
            TypeMap[typeof(long?)] = DbType.Int64;
            TypeMap[typeof(ulong?)] = DbType.UInt64;
            TypeMap[typeof(float?)] = DbType.Single;
            TypeMap[typeof(double?)] = DbType.Double;
            TypeMap[typeof(Decimal?)] = DbType.Decimal;
            TypeMap[typeof(bool?)] = DbType.Boolean;
            TypeMap[typeof(char?)] = DbType.StringFixedLength;
            TypeMap[typeof(Guid?)] = DbType.Guid;
            TypeMap[typeof(DateTime?)] = DbType.DateTime;
            TypeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            TypeMap[typeof(TimeSpan?)] = DbType.Time;
            TypeMap[typeof(object)] = DbType.Object;
            TypeMap[typeof(Uri)] = DbType.String;
        }

        public static DbType AsDbType(Type propertyType)
        {
            DbType dbType;
            if (!TypeMap.TryGetValue(propertyType, out dbType))
                throw new KeyNotFoundException("Cannot map database type from: " + propertyType.FullName);
            return dbType;
        }
    }
}
