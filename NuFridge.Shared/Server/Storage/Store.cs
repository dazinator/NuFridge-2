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

        public Store(IContainer container, IHomeConfiguration config, string connectionString, RelationalMappings mappings)
        {
            _container = container;
            _config = config;
            _mappings = mappings;
            _connectionString = SetConnectionStringOptions(connectionString);
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

        private string SetConnectionStringOptions(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(_config.SqlDataSource))
            {
                throw new Exception("No SQL Server settings were found in the config file.");
            }

            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                MultipleActiveResultSets = true,
                Enlist = false,
                Pooling = true,
                ApplicationName = "NuFridge",
                DataSource = _config.SqlDataSource,
                InitialCatalog = _config.SqlInitialCatalog
            };

            if (_config.SqlAuthenticationMode == HomeConfiguration.SqlAuthentication.UserIdPasswordAuthentication)
            {
                connectionStringBuilder.UserID = _config.SqlUsername;
                connectionStringBuilder.Password = _config.SqlPassword;
                connectionStringBuilder.IntegratedSecurity = false;
            }
            else
            {
                connectionStringBuilder.IntegratedSecurity = true;
            }

            return connectionStringBuilder.ToString();
        }
    }
}
