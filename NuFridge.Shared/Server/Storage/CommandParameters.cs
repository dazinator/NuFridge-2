using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace NuFridge.Shared.Server.Storage
{
    public class CommandParameters : Dictionary<string, object>
    {
        public CommandType CommandType { get; set; }

        public CommandParameters()
            : base(StringComparer.OrdinalIgnoreCase)
        {
            CommandType = CommandType.Text;
        }

        public CommandParameters(object args)
            : this()
        {
            AddFromParametersObject(args);
        }

        private void AddFromParametersObject(object args)
        {
            if (args == null)
                return;
            Type type = args.GetType();
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                object obj = PropertyReaderFactory.Create<object>(type, propertyInfo.Name).Read(args);
                this[propertyInfo.Name] = obj;
            }
        }

        public virtual void ContributeTo(SqlCommand command)
        {
            command.CommandType = CommandType;
            foreach (KeyValuePair<string, object> keyValuePair in this)
                ContributeParameter(command, keyValuePair.Key, keyValuePair.Value);
        }

        protected virtual void ContributeParameter(SqlCommand command, string name, object value)
        {
            if (value == null)
                command.Parameters.Add(new SqlParameter(name, DBNull.Value));
            else if (value is IEnumerable && !(value is string) && !(value is byte[]))
            {
                List<string> list = new List<string>();
                int num = 0;
                foreach (object obj in (IEnumerable)value)
                {
                    string name1 = name + "_" + num;
                    list.Add(name1);
                    ContributeParameter(command, name1, obj);
                    ++num;
                }
                if (num == 0)
                {
                    string name1 = name + "_" + num;
                    list.Add(name1);
                    ContributeParameter(command, name1, null);
                }
                command.CommandText = command.CommandText.Replace("@" + name.TrimStart('@'), "(" + string.Join(", ", list.Select(x => "@" + x)) + ")");
            }
            else
            {
                DbType dbType = DatabaseTypeConverter.AsDbType(value.GetType());
                SqlParameter sqlParameter = new SqlParameter();
                sqlParameter.ParameterName = name;
                sqlParameter.DbType = dbType;
                sqlParameter.Value = value;
                command.Parameters.Add(sqlParameter);
            }
        }
    }
}
