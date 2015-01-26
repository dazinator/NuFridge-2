using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using NuFridge.DataAccess.Model;

namespace NuFridge.DataAccess.Repositories
{
    public class NuFridgeContext : DbContext
    {
        public DbSet<Feed> Feeds { get; set; }
        public DbSet<FeedGroup> FeedGroups { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //   modelBuilder.Entity<AccountEntity>().HasKey(a=>a.)
            base.OnModelCreating(modelBuilder);
        }
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

        public SqlCompactRepository()
        {
        }

        public bool Insert(TEntity entity)
        {
            using (var context = new NuFridgeContext())
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
            using (var context = new NuFridgeContext())
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
            using (var context = new NuFridgeContext())
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
            using (var context = new NuFridgeContext())
            {
                var collection = context.Set<TEntity>();
                return collection.ToList();
            }
        }

        public TEntity GetById(Guid id)
        {
            using (var context = new NuFridgeContext())
            {
                var collection = context.Set<TEntity>();
                return collection.Find(id);
            }
        }

        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            using (var context = new NuFridgeContext())
            {
                var collection = context.Set<TEntity>();
                return collection.AsQueryable().Where(predicate);
            }
        }

    }
}
