using System.Collections.Generic;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public interface IJobTypeRepository<in T> where T : class
    {
        bool IsTypeCompatible<TRecord>() where TRecord : class, new();
        void Insert(T job);
        void Update(T job);
        TRecord Find<TRecord>(int jobId) where TRecord : class, IJobType, new();
        IEnumerable<TRecord> FindForFeed<TRecord>(int feedId) where TRecord : class, IJobType, new();
    }

    public interface IJobType
    {
        int JobId { get; set; }
    }
}