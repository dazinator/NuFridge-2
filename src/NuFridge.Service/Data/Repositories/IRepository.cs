using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace NuFridge.Service.Data.Repositories
{
    public interface IRepository<TEntity>
    {
        bool Insert(TEntity entity);
        bool Update(TEntity entity);
        bool Delete(TEntity entity);
        IList<TEntity> GetAll();
        TEntity GetById(Guid id);
        DbContext Context { get;}
    }
}
