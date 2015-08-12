using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using NCrontab;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Scheduler.Jobs;
using NuFridge.Shared.Server.Scheduler.Servers;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler
{
    public class JobServerManager : IJobServerManager
    {
        private readonly ILog _log = LogProvider.For<JobServerManager>();
        private readonly IStore _store;
        private readonly IContainer _container;

        public JobServerManager(IContainer container, IStore store)
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

            updateStatusAction("Shutting down the job servers");

            IEnumerable<JobServerInstance> jobServers = _container.Resolve<IEnumerable<JobServerInstance>>();

            foreach (var jobServerInstance in jobServers)
            {
                jobServerInstance.Stop();
                _log.Info("Successfully stopped job server for " + jobServerInstance.QueueName);
            }
        }

        public void Start(Action<string> updateStatusAction)
        {
            updateStatusAction("Starting the job scheduler");

            _log.Info("Starting the job scheduler");
            var options = new SqlServerStorageOptions {PrepareSchemaIfNecessary = true};
            GlobalConfiguration.Configuration.UseSqlServerStorage(_store.ConnectionString, options).UseMsmqQueues(@".\Private$\nufridge_{0}", "filesystem", "background", "download");
            GlobalConfiguration.Configuration.UseAutofacActivator(_container);
            HangfirePerLifetimeScopeConfigurer.Configure(_container);
            GlobalConfiguration.Configuration.UseFilter(new JobContext());

            var monitorApi = JobStorage.Current.GetMonitoringApi();

            var processingJobs = monitorApi.ProcessingJobs(0, int.MaxValue);

            if (processingJobs.Any())
            {
                foreach (var processingJob in processingJobs)
                {
                    var jobDetails = monitorApi.JobDetails(processingJob.Key);
                    if (jobDetails.History.Any(hi => hi.Data.ContainsKey("Queue") && hi.Data["Queue"] == "background" ))
                    {
                        BackgroundJob.Delete(processingJob.Key);
                    }
                }
            }

            var scheduledJobs = monitorApi.ScheduledJobs(0, int.MaxValue).ToList();

            if (scheduledJobs.Any())
            {
                foreach (var scheduledJob in scheduledJobs)
                {
                    var jobDetails = monitorApi.JobDetails(scheduledJob.Key);
                    if (jobDetails.History.Any(hi => hi.Data.ContainsKey("Queue") && hi.Data["Queue"] == "background"))
                    {
                        BackgroundJob.Delete(scheduledJob.Key);
                    }
                }
            }

            IEnumerable<JobServerInstance> jobServers = _container.Resolve<IEnumerable<JobServerInstance>>();

            foreach (var jobServerInstance in jobServers)
            {
                jobServerInstance.Start(monitorApi, updateStatusAction);
                _log.Info("Successfully started job server for " + jobServerInstance.QueueName);
            }

            List<JobBase> tasks = _container.Resolve<IEnumerable<JobBase>>().ToList();


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

                var cronSchedule = CrontabSchedule.Parse(jobBase.Cron);
                var nextExecuting = DateTime.MinValue;

                var lastExecution = job.LastExecution;
                if (lastExecution.HasValue)
                {
                    nextExecuting = cronSchedule.GetNextOccurrence(lastExecution.Value);

                    var minutesUntilNextRun = Math.Round(nextExecuting.Subtract(DateTime.UtcNow).TotalMinutes, MidpointRounding.ToEven);

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

            Thread.Sleep(1000);

            WaitForEnqueuedTaskToComplete(monitorApi, jobIdToWaitFor, updateStatusAction);
        }
    }

    public interface IJobServerManager
    {
        void Start(Action<string> updateStatusAction);
        void Stop(Action<string> updateStatusAction);
    }
}