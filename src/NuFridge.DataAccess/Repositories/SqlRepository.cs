using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NuFridge.DataAccess.Model;

namespace NuFridge.DataAccess.Repositories
{
    public class NuFridgeContext : DbContext
    {
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

    /// <summary>
    /// A SqlCompact repository. Maps to a collection with the same name
    /// as type TEntity.
    /// </summary>
    /// <typeparam name="T">Entity type for this repository</typeparam>
    public class SqlCompactRepository<TEntity> :
        IRepository<TEntity> where
            TEntity : class, IEntityBase
    {
        private ConnectionStringSettings connectionString { get; set; }

        public SqlCompactRepository()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"];
        }

        public bool Insert(TEntity entity)
        {
            using (var context = new NuFridgeContext(connectionString))
            {
                entity.Id = Guid.NewGuid();
                var collection = context.Set<TEntity>();
                collection.Add(entity);
                var result = context.SaveChanges();
                return true;
            }

        }

        public bool Update(TEntity entity)
        {
            using (var context = new NuFridgeContext(connectionString))
            {
                entity.Id = Guid.NewGuid();
                var collection = context.Set<TEntity>();
                if (entity.Id == null)
                    return Insert(entity);
                collection.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
                var result = context.SaveChanges();
                return true;
            }
        }

        public bool Delete(TEntity entity)
        {
            using (var context = new NuFridgeContext(connectionString))
            {
                TEntity existing;
                var collection = context.Set<TEntity>();
                if (context.Entry(entity).State == EntityState.Detached)
                {
                    existing = collection.Find(entity.Id);
                }
                else
                {
                    existing = entity;
                }
                context.Entry(existing).State = EntityState.Deleted;
                var result = context.SaveChanges();
                return true;
            }
        }

        public IList<TEntity> GetAll()
        {
            using (var context = new NuFridgeContext(connectionString))
            {
                var collection = context.Set<TEntity>();
                return collection.ToList();
            }
        }

        public TEntity GetById(Guid id)
        {
            using (var context = new NuFridgeContext(connectionString))
            {
                var collection = context.Set<TEntity>();
                return collection.Find(id);
            }
        }

        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            using (var context = new NuFridgeContext(connectionString))
            {
                var collection = context.Set<TEntity>();
                return collection.AsQueryable().Where(predicate);
            }
        }

    }
}
