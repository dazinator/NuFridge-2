using System;
using System.Data.Entity;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Database.Model
{
    public class DatabaseContext : DbContext
    {
        public static readonly Lazy<string> ConnectionString = new Lazy<string>(GetConnectionString);

        public DatabaseContext() : base(ConnectionString.Value)
        {
            Configuration.AutoDetectChangesEnabled = false;
        }

        private static string GetConnectionString()
        {
            var conn =  GlobalHost.DependencyResolver.Resolve<IHomeConfiguration>().ConnectionString;

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