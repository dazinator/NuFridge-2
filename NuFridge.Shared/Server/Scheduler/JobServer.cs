using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using Hangfire.Server;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Scheduler.Jobs;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler
{
    public class JobServer : IJobServer
    {
        private readonly ILog _log = LogProvider.For<JobServer>();
        private BackgroundJobServer _backgroundJobServer;
        public BackgroundJobServerOptions BackgroundJobServerOptions { get; private set; }
        private readonly IStore _store;
        private readonly IContainer _container;

        public JobServer(IContainer container, IStore store)
        {
            _container = container;
            _store = store;
        }

        public void Stop(Action<string> updateStatusAction)
        {
            _log.Info("Shutting down the job server. Any jobs being processed will be terminated after 3 minutes.");

            var monitorApi = JobStorage.Current.GetMonitoringApi();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            bool jobsRanForOver3Mins = false;

            while (monitorApi.ProcessingCount() > 0 && !jobsRanForOver3Mins)
            {
                if (watch.Elapsed.TotalMinutes >= 3)
                {
                    jobsRanForOver3Mins = true;
                }

                var processingCount = monitorApi.ProcessingCount();

                updateStatusAction("Waiting for " + processingCount + " jobs to complete to continue shutdown");

                _log.Info("Waiting for " + processingCount + " jobs to complete.");

                Thread.Sleep(5000);
            }

            watch.Stop();

            var processingJobs = monitorApi.ProcessingJobs(0, 10);

            foreach (var processingJob in processingJobs)
            {
                _log.Warn("The " + processingJob.Value.Job.Type.Name + " job will be terminated as it execeeded the 3 minute timeout.");
            }

            updateStatusAction("Shutting down the job server");

            _backgroundJobServer.Dispose();
        }

        public void Start(Action<string> updateStatusAction)
        {
            updateStatusAction("Starting the job scheduler");

            _log.Info("Starting the job scheduler");
            var options = new SqlServerStorageOptions {PrepareSchemaIfNecessary = true};
            GlobalConfiguration.Configuration.UseSqlServerStorage(_store.ConnectionString, options).UseMsmqQueues(@".\Private$\nufridge_{0}", "filesystem", "background");
            GlobalConfiguration.Configuration.UseAutofacActivator(_container);
            HangfirePerLifetimeScopeConfigurer.Configure(_container);
            GlobalConfiguration.Configuration.UseFilter(new JobContext());


            var monitorApi = JobStorage.Current.GetMonitoringApi();

            var processingJobs = monitorApi.ProcessingJobs(0, int.MaxValue);

            if (processingJobs.Any())
            {
                updateStatusAction("Deleting " + processingJobs.Count() + " jobs which are currently stuck processing. This happens when NuFridge is shutdown during job execution.");

                _log.Warn("Deleting " + processingJobs.Count() + " jobs which are currently stuck processing. This happens when NuFridge is shutdown during job execution.");

                foreach (var processingJob in processingJobs)
                {
                    BackgroundJob.Delete(processingJob.Key);
                }
            }

            
            var queuedJobs =
                monitorApi.EnqueuedJobs("filesystem", 0, int.MaxValue)
                    .Union(monitorApi.EnqueuedJobs("background", 0, int.MaxValue)).ToList();

            if (queuedJobs.Any())
            {

                _log.Warn("Deleting " + queuedJobs.Count() + " jobs which are currently queued.");

                updateStatusAction("Deleting " + queuedJobs.Count() + " jobs which are currently queued.");

                foreach (var queuedJob in queuedJobs)
                {
                    BackgroundJob.Delete(queuedJob.Key);
                }
            }


            var fetchedJobs =
                monitorApi.FetchedJobs("filesystem", 0, int.MaxValue)
                    .Union(monitorApi.FetchedJobs("background", 0, Int32.MaxValue))
                    .ToList();


            if (fetchedJobs.Any())
            {
                _log.Warn("Deleting " + fetchedJobs.Count() + " jobs which are currently feteched.");

                updateStatusAction("Deleting " + fetchedJobs.Count() + " jobs which are currently fetched.");

                foreach (var fetchedJob in fetchedJobs)
                {
                    BackgroundJob.Delete(fetchedJob.Key);
                }
            }

            var scheduledJobs = monitorApi.ScheduledJobs(0, int.MaxValue).ToList();

            if (scheduledJobs.Any())
            {
                _log.Warn("Deleting " + scheduledJobs.Count() + " jobs which are currently scheduled.");

                updateStatusAction("Deleting " + scheduledJobs.Count() + " jobs which are currently scheduled.");

                foreach (var scheduledJob in scheduledJobs)
                {
                    BackgroundJob.Delete(scheduledJob.Key);
                }
            }

            BackgroundJobServerOptions = new BackgroundJobServerOptions
            {
                Queues = new[] {"background", "filesystem"},
                ServerName = Environment.MachineName
            };

            _backgroundJobServer = new BackgroundJobServer(BackgroundJobServerOptions);

            _log.Info("Successfully started job server");

            var tasks = _container.Resolve<IEnumerable<JobBase>>().ToList();


            updateStatusAction("Adding " + tasks.Count() + " jobs to the scheduler");

            foreach (var task in tasks)
            {
                _log.Info("Scheduling recurring job " + task.GetType().Name);

                RecurringJob.AddOrUpdate(task.JobId, () => task.Execute(JobCancellationToken.Null), task.Cron);
            }

            var jobsToRunNow = tasks.Where(tk => tk.TriggerOnRegister).ToList();

            updateStatusAction("Enqueuing " + jobsToRunNow.Count() + " jobs");

            List<string> jobIdsToWaitFor = new List<string>();

            foreach (var jobBase in jobsToRunNow)
            {
                RecurringJobDto job;

                using (IStorageConnection connection = JobStorage.Current.GetConnection())
                {
                    job = connection.GetRecurringJobs().First(jb => jb.Id == jobBase.JobId);
                }

                var cronSchedule = NCrontab.CrontabSchedule.Parse(jobBase.Cron);
                var nextExecuting = DateTime.MinValue;

                var lastExecution = job.LastExecution;
                if (lastExecution.HasValue)
                {
                    nextExecuting = cronSchedule.GetNextOccurrence(lastExecution.Value);

                    var minutesUntilNextRun = nextExecuting.Subtract(DateTime.UtcNow).TotalMinutes;

                    if (minutesUntilNextRun > 1)
                    {
                        _log.Debug("Executing the " + jobBase.JobId + " job as the next scheduled run is " + minutesUntilNextRun +
                                   " minutes away");

                        jobIdsToWaitFor.Add(BackgroundJob.Enqueue(() => jobBase.Execute(JobCancellationToken.Null)));
                    }
                    else
                    {
                        _log.Debug("Not executing the " + jobBase.JobId + " job as the next scheduled run is " + minutesUntilNextRun + " minutes away");
                    }
                }
                else
                {
                    _log.Debug("Executing the " + jobBase.JobId + " job as it has not been run before");

                    jobIdsToWaitFor.Add(BackgroundJob.Enqueue(() => jobBase.Execute(JobCancellationToken.Null)));
                }
            }

            foreach (var jobIdToWaitFor in jobIdsToWaitFor)
            {
                WaitForEnqueuedTaskToComplete(monitorApi, jobIdToWaitFor, updateStatusAction);
            }
        }

        private void WaitForEnqueuedTaskToComplete(IMonitoringApi monitorApi, string jobIdToWaitFor, Action<string> updateStatusAction)
        {
            var job = monitorApi.JobDetails(jobIdToWaitFor);

            if (job.History.Any(it => it.StateName == FailedState.StateName))
            {
                throw new Exception("The " + job.Job.Type.Name + " job failed to execute");
            }
            if (job.History.Any(it => it.StateName == SucceededState.StateName))
            {
                _log.Info("The " + job.Job.Type.Name + " job has finished");
                return;
            }

            _log.Info("Waiting for the " + job.Job.Type.Name + " job to complete");
            updateStatusAction("Waiting for the " + job.Job.Type.Name + " job to complete");

            if (job.History.Any())
            {
                _log.Info("The " + job.Job.Type.Name + " job state is " + job.History[0].StateName);
            }

            Thread.Sleep(2000);

            WaitForEnqueuedTaskToComplete(monitorApi, jobIdToWaitFor, updateStatusAction);
        }
    }

    public interface IJobServer
    {
        BackgroundJobServerOptions BackgroundJobServerOptions { get; }
        void Start(Action<string> updateStatusAction);
        void Stop(Action<string> updateStatusAction);
    }
}
