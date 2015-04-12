using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Reflection;
using Microsoft.AspNet.Identity.EntityFramework;
using NuFridge.Service.Authentication.Model;
using NuFridge.Service.Data.Model;
using NuFridge.Service.Logging;
using Configuration = NuFridge.Service.Migrations.Configuration;

namespace NuFridge.Service.Data.Repositories
{
    public class NuFridgeContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
    {
        private static readonly ILog Logger = LogProvider.For<NuFridgeContext>(); 

       // new public virtual IDbSet<ApplicationRole> Roles { get; set; }

        public virtual IDbSet<ApplicationGroup> Groups { get; set; }

        public NuFridgeContext(ConnectionStringSettings connectionStringSettings)
            : base(connectionStringSettings.ConnectionString)
        {
            ConfigureSqlServerCeDatabase(connectionStringSettings.ConnectionString);
        }

        public NuFridgeContext()
        {
            ConfigureSqlServerCeDatabase(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
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

        public static bool TryUpgrade()
        {
            Logger.Info("Starting database upgrade.");

            using (var context = new NuFridgeContext(ConfigurationManager.ConnectionStrings["DefaultConnection"]))
            {

                Database.SetInitializer(new MigrateDatabaseToLatestVersion<NuFridgeContext, Configuration>());

                try
                {
                    context.Database.Initialize(false);

                    Logger.Info("Finished database upgrade.");

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

                        Logger.Info("Failed to perform database upgrade: " + ceException.Message);
                        Logger.Info(baseException.StackTrace);
                    }
                    else
                    {
                        Logger.Info("Failed to perform database upgrade: " + baseException.Message);
                        Logger.Info(baseException.StackTrace);
                    }

                    return false;
                }
            }
        }

        private static void ProcessValidationException(DbEntityValidationException ex)
        {
            Logger.Info("Failed to perform database upgrade:");

            foreach (var entityError in ex.EntityValidationErrors)
            {
                Logger.Info("Entity: " + entityError.Entry.Entity.GetType().Name);
                foreach (var validationError in entityError.ValidationErrors)
                {
                    Logger.Info("    " + validationError.ErrorMessage);
                }
            }
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
                var assemblyPath = Assembly.GetEntryAssembly().Location;
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
                        Logger.Info("Creating a directory for the database at " + databaseFolder.FullName + ".");
                        Directory.CreateDirectory(databaseFolder.FullName);
                    }

                    Logger.Info("Creating the SQL CE database at "+ databasePath + ".");

                    //Create the SQL CE database
                    objCeEngine.CreateDatabase();
                }
            }
        }

        public DbSet<Feed> Feeds { get; set; }
        public DbSet<FeedGroup> FeedGroups { get; set; }
    }
}