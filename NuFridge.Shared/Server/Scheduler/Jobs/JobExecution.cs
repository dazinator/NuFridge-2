using System;
using Hangfire;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public abstract class JobExecution<T> where T : class, IJobType, new()
    {
        private readonly IJobService _jobService;
        protected T Job { get; private set; }
        private Job JobBase { get; set; }

        protected int? FeedId { get; private set; }
        protected int UserId { get; private set; } 
        protected string TaskName { get; private set; }
        protected IJobCancellationToken CancellationToken { get; private set; }

        protected void WithFeedId(int feedId)
        {
            FeedId = feedId;
        }

        protected void WithCancellationToken(IJobCancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        protected void WithUserId(int userId)
        {
            UserId = userId;
        }

        protected void WithTaskName(string name)
        {
            TaskName = name;
        }

        protected JobExecution(IJobService jobService)
        {
            _jobService = jobService;
        }

        protected abstract void Execute();

        protected void SaveJob()
        {
            _jobService.Update(Job);
        }

        protected void CancelJob()
        {
            JobBase.CompletedAt = DateTime.UtcNow;
            JobBase.Success = false;

            _jobService.Update(JobBase);
            _jobService.Update(Job);
        }

        protected void Start()
        {
            BeforeStart();

            Execute();

            AfterStart();
        }

        private void BeforeStart()
        {
            int jobId = int.Parse(JobContext.JobId);

            JobBase = _jobService.Find(jobId);

            if (JobBase == null)
            {
                JobBase = new Job
                {
                    CreatedAt = DateTime.UtcNow,
                    HasWarnings = false,
                    RetryCount = 0,
                    UserId = UserId,
                    CompletedAt = null,
                    FeedId = FeedId,
                    Id = jobId,
                    Name = TaskName
                };

                _jobService.Insert(JobBase);
            }
            else
            {
                JobBase.RetryCount++;
                _jobService.Update(JobBase);
            }

            Job = _jobService.Find<T>(jobId);

            if (Job == null)
            {
                Job = new T();
                Job.JobId = jobId;

                _jobService.Insert(Job);
            }
        }

        private void AfterStart()
        {
            JobBase.CompletedAt = DateTime.UtcNow;
            JobBase.Success = true;

            _jobService.Update(JobBase);
            _jobService.Update(Job);
        }
    }
}