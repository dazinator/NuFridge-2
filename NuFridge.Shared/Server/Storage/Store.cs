using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Storage
{
    public class Store : IStore
    {
        private readonly IHomeConfiguration _config;
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings();
        private readonly RelationalMappings _mappings;
        private readonly string _connectionString;
        private readonly IContainer _container;

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public Store(IContainer container, IHomeConfiguration config, RelationalMappings mappings)
        {
            _container = container;
            _config = config;
            _mappings = mappings;
            _connectionString = SetConnectionStringOptions();
            _jsonSettings.ContractResolver = new RelationalJsonContractResolver(mappings);
            _jsonSettings.Converters.Add(new StringEnumConverter());
            _jsonSettings.Converters.Add(new VersionConverter());
            _jsonSettings.DateFormatHandling = 0;
            _jsonSettings.DateTimeZoneHandling = (DateTimeZoneHandling)3;
            _jsonSettings.TypeNameHandling = (TypeNameHandling)4;
            _jsonSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
        }

        public ITransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return new Transaction(_container, _connectionString, isolationLevel, _jsonSettings, _mappings);
        }

        private string SetConnectionStringOptions()
        {
            if (string.IsNullOrWhiteSpace(_config.ConnectionString))
            {
                throw new Exception("No SQL Server connection string was set in the configuration file.");
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder(_config.ConnectionString)
            {
                MultipleActiveResultSets = true,
                Enlist = false,
                Pooling = true,
                ApplicationName = "NuFridge"
            };

            return connectionStringBuilder.ToString();
        }
    }
}
