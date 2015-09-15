using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        private readonly IEnumerable<IJobTypeRepository<IJobType>> _jobTypeRepositories;

        public JobService(IJobRepository jobRepository, IEnumerable<IJobTypeRepository<IJobType>> jobTypeRepositories)
        {
            _jobRepository = jobRepository;
            _jobTypeRepositories = jobTypeRepositories;
        }

        private IJobTypeRepository<IJobType> FindTypeRepository<T>() where T : class, new()
        {
            foreach (var jobTypeRepository in _jobTypeRepositories)
            {
                if (jobTypeRepository.IsTypeCompatible<T>())
                {
                    return jobTypeRepository;
                }
            }
            
            throw new NotSupportedException("No " + typeof(IJobTypeRepository<T>).Name + " implementation found for type " + typeof(T).Name);
        }


        public IEnumerable<Job> FindForFeed(int feedId, int pageNumber, int rows, out int totalResults)
        {
            return _jobRepository.FindForFeed(feedId, pageNumber, rows, out totalResults);
        }

        public IEnumerable<Job> FindForFeed(int feedId)
        {
            return _jobRepository.FindForFeed(feedId);
        }

        public IEnumerable<T> FindForFeed<T>(int feedId) where T : class, IJobType, new()
        {
            IJobTypeRepository<IJobType> repo = FindTypeRepository<T>();
            return repo.FindForFeed<T>(feedId);
        }

        public void Insert<T>(T job) where T : class, IJobType, new()
        {
            IJobTypeRepository<IJobType> repo = FindTypeRepository<T>();
            repo.Insert(job);
        }

        public void Insert(Job job)
        {
            _jobRepository.Insert(job);
        }

        public T Find<T>(int jobId) where T : class, IJobType, new()
        {
            IJobTypeRepository<IJobType> repo = FindTypeRepository<T>();
            return repo.Find<T>(jobId);
        }

        public Job Find(int jobId)
        {
            return _jobRepository.Find(jobId);
        }

        public IEnumerable<Job> Find(int pageNumber, int rows, out int totalResults)
        {
            return _jobRepository.Find(pageNumber, rows, out totalResults);
        }

        public void Update(Job job)
        {
            _jobRepository.Update(job);
        }

        public void Update<T>(T job) where T : class, IJobType, new()
        {
            IJobTypeRepository<IJobType> repo = FindTypeRepository<T>();
            repo.Update(job);
        }
    }

    public interface IJobService
    {
        void Insert<T>(T job) where T : class, IJobType, new();
        void Update<T>(T job) where T : class, IJobType, new();
        void Update(Job job);
        void Insert(Job job);
        Job Find(int jobId);
        T Find<T>(int jobId) where T : class, IJobType, new();
        IEnumerable<Job> FindForFeed(int feedId);
        IEnumerable<T> FindForFeed<T>(int feedId) where T : class, IJobType, new();
        IEnumerable<Job> FindForFeed(int feedId, int pageNumber, int rows, out int totalResults);
        IEnumerable<Job> Find(int pageNumber, int rows, out int totalResults);
    }
}