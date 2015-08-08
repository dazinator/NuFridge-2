using System;
using System.Data.SqlClient;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Storage
{
    public class Store : IStore
    {
        private readonly IHomeConfiguration _config;

        public string ConnectionString { get; }

        public Store(IHomeConfiguration config)
        {
            _config = config;
            ConnectionString = SetConnectionStringOptions();
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
                Enlist = true,
                Pooling = true,
                ApplicationName = "NuFridge"
            };

            return connectionStringBuilder.ToString();
        }
    }
}
