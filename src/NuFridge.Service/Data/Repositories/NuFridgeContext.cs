using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using NuFridge.Service.Authentication;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Data.Model;

namespace NuFridge.Service.Data.Repositories
{
    public class NuFridgeContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
       // new public virtual IDbSet<ApplicationRole> Roles { get; set; }

        public virtual IDbSet<ApplicationGroup> Groups { get; set; }

        public NuFridgeContext(ConnectionStringSettings connectionStringSettings)
            : base(connectionStringSettings.ConnectionString)
        {
            const string sqlServerCeProvideName = "System.Data.SqlServerCe.4.0";
            const string sqlServerProviderName = "System.Data.SqlClient";



            if (connectionStringSettings.ProviderName == sqlServerCeProvideName)
            {
                ConfigureSqlServerCeDatabase(connectionStringSettings.ConnectionString);
            }
            else if (connectionStringSettings.ProviderName == sqlServerProviderName)
            {
                ConfigureSqlServerDatabase(connectionStringSettings);
            }
            else
            {
                throw new InvalidOperationException(string.Format("The '{0}' provider is not supported for connecting to the database.", connectionStringSettings.ProviderName));
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException("modelBuilder");
            }
            // Keep this:
            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");
            // Change TUser to ApplicationUser everywhere else - IdentityUser 
            // and ApplicationUser essentially 'share' the AspNetUsers Table in the database:
            EntityTypeConfiguration<ApplicationUser> table =
                modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            table.Property((ApplicationUser u) => u.UserName).IsRequired();
            // EF won't let us swap out IdentityUserRole for ApplicationUserRole here:
            modelBuilder.Entity<ApplicationUser>().HasMany<IdentityUserRole>((ApplicationUser u) => u.Roles);
            modelBuilder.Entity<IdentityUserRole>().HasKey((IdentityUserRole r) =>
                new { UserId = r.UserId, RoleId = r.RoleId }).ToTable("AspNetUserRoles");
            // Add the group stuff here:
            modelBuilder.Entity<ApplicationUser>().HasMany<ApplicationUserGroup>((ApplicationUser u) => u.Groups);
            modelBuilder.Entity<ApplicationUserGroup>().HasKey((ApplicationUserGroup r) =>
                new { UserId = r.UserId, GroupId = r.GroupId }).ToTable("ApplicationUserGroups");
            // And here:
            modelBuilder.Entity<ApplicationGroup>().HasMany<ApplicationRoleGroup>((ApplicationGroup g) => g.Roles);
            modelBuilder.Entity<ApplicationRoleGroup>().HasKey((ApplicationRoleGroup gr) =>
                new { RoleId = gr.RoleId, GroupId = gr.GroupId }).ToTable("ApplicationRoleGroups");
            // And Here:
            EntityTypeConfiguration<ApplicationGroup> groupsConfig = modelBuilder.Entity<ApplicationGroup>().ToTable("Groups");
            groupsConfig.Property((ApplicationGroup r) => r.Name).IsRequired();
            // Leave this alone:
            EntityTypeConfiguration<IdentityUserLogin> entityTypeConfiguration =
                modelBuilder.Entity<IdentityUserLogin>().HasKey((IdentityUserLogin l) =>
                    new
                    {
                        UserId = l.UserId,
                        LoginProvider = l.LoginProvider,
                        ProviderKey =
                            l.ProviderKey
                    }).ToTable("AspNetUserLogins");
        
            EntityTypeConfiguration<IdentityUserClaim> table1 =
                modelBuilder.Entity<IdentityUserClaim>().ToTable("AspNetUserClaims");

            // Add this, so that IdentityRole can share a table with ApplicationRole:
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");
            // Change these from IdentityRole to ApplicationRole:
            EntityTypeConfiguration<ApplicationRole> entityTypeConfiguration1 =
                modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles");
            entityTypeConfiguration1.Property((ApplicationRole r) => r.Name).IsRequired();
        }

        public bool Upgrade()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<NuFridgeContext, Migrations.Configuration>());

            try
            {
                Database.Initialize(false);

                return true;
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();

                if (baseException is DbEntityValidationException)
                {
                    ProcessValidationException(baseException as DbEntityValidationException);
                }
                else if (baseException is SqlCeException)
                {
                    var ceException = baseException as SqlCeException;

                    Console.WriteLine("Failed to perform database upgrade: " + ceException.Message);
                    Console.WriteLine(baseException.StackTrace);
                }
                else
                {
                    Console.WriteLine("Failed to perform database upgrade: " + baseException.Message);
                    Console.WriteLine(baseException.StackTrace);
                }

                return false;
            }
        }

        private void ProcessValidationException(DbEntityValidationException ex)
        {
            Console.WriteLine("Failed to perform database upgrade:");

            foreach (var entityError in ex.EntityValidationErrors)
            {
                Console.WriteLine("Entity: " + entityError.Entry.Entity.GetType().Name);
                foreach (var validationError in entityError.ValidationErrors)
                {
                    Console.WriteLine("    " + validationError.ErrorMessage);
                }
            }
        }

        public NuFridgeContext()
        {

        }

        private void ConfigureSqlServerDatabase(ConnectionStringSettings connectionStringSettings)
        {

        }

        private void ConfigureSqlServerCeDatabase(string connectionString)
        {
            //Replace data directory with the current working folder
            var updatedConnection = connectionString.Replace("|DataDirectory|",
                AppDomain.CurrentDomain.GetData("APPBASE").ToString());

            //Parse connection string
            var sqlBuilder = new SqlConnectionStringBuilder(updatedConnection);

            //Get file path
            string databasePath = sqlBuilder.DataSource;

            //If the path is not absolute
            if (!Path.IsPathRooted(databasePath))
            {
                var assemblyPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                var assemblyFolder = Directory.GetParent(assemblyPath).FullName;

                databasePath = Path.Combine(assemblyFolder, databasePath);
            }

            //If the database file does not exist
            if (!File.Exists(databasePath))
            {
                using (var objCeEngine = new SqlCeEngine(connectionString))
                {
                    //Create database folder if it does not exist
                    var databaseFolder = Directory.GetParent(databasePath);
                    if (!databaseFolder.Exists)
                    {
                        Directory.CreateDirectory(databaseFolder.FullName);
                    }

                    //Create the SQL CE database
                    objCeEngine.CreateDatabase();
                }
            }
        }

        public DbSet<Feed> Feeds { get; set; }
        public DbSet<FeedGroup> FeedGroups { get; set; }
    }

}
