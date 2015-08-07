using System;
using System.Data.Entity;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Server.Configuration;

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
                //.MapToStoredProcedures(s => s.Insert(u => u.HasName("InsertPackage", "NuFridge"))
                //                            .Update(u => u.HasName("UpdatePackage", "NuFridge"))
                //                            .Delete(u => u.HasName("DeletePackage", "NuFridge")));

            modelBuilder.Entity<Feed>()
                .ToTable("Feed", "NuFridge")
                .HasKey(feed => feed.Id);

            modelBuilder.Entity<FeedConfiguration>()
                .ToTable("FeedConfiguration", "NuFridge")
                .HasKey(feedconfig => feedconfig.Id);

            modelBuilder.Entity<Statistic>()
                .ToTable("Statistic", "NuFridge")
                .HasKey(stat => stat.Id);

            modelBuilder.Entity<User>()
                .ToTable("User", "NuFridge")
                .HasKey(user => user.Id);

            modelBuilder.Entity<Framework>()
                .ToTable("Framework", "NuFridge")
                .HasKey(framework => framework.Id);
        }

        public DbSet<InternalPackage> Packages { get; set; }
        public DbSet<Feed> Feeds { get; set; }
        public DbSet<FeedConfiguration> FeedConfigurations { get; set; }
        public DbSet<Statistic> Statistics { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Framework> Frameworks { get; set; }
    }
}