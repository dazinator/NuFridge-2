using System;
using System.Data.Entity;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Database.Model
{
    public class DatabaseContext : DbContext
    {
        private readonly IHomeConfiguration _homeConfiguration;

        public DatabaseContext(IHomeConfiguration homeConfiguration)
        {
            _homeConfiguration = homeConfiguration;
            Configuration.AutoDetectChangesEnabled = false;
        }

        public bool GetReadOnly()
        {
            return _homeConfiguration.DatabaseReadOnly;
        }

        public string GetConnectionString()
        {
            var conn = _homeConfiguration.ConnectionString;

            var connectionStringBuilder = new SqlConnectionStringBuilder(conn)
            {
                MultipleActiveResultSets = true,
                Enlist = true,
                Pooling = true,
                ApplicationName = "NuFridge",
                AsynchronousProcessing = true
            };

            return connectionStringBuilder.ToString();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InternalPackage>()
                .ToTable("Package", "NuFridge")
                .HasKey(package => package.PrimaryId);
        }
    }
}