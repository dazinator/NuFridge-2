using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
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

                updateStatusAction("Waiting for " + processingCount + " jobs to complete");

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
            _log.Info("Starting job server");

            var options = new SqlServerStorageOptions {PrepareSchemaIfNecessary = true};
            GlobalConfiguration.Configuration.UseSqlServerStorage(_store.ConnectionString, options);
            GlobalConfiguration.Configuration.UseAutofacActivator(_container);
            HangfirePerLifetimeScopeConfigurer.Configure(_container);
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute {Attempts = 0});
            BackgroundJobServerOptions = new BackgroundJobServerOptions();

            _backgroundJobServer = new BackgroundJobServer(BackgroundJobServerOptions);

            _log.Info("Successfully started job server");

            var tasks = _container.Resolve<IEnumerable<JobBase>>().ToList();

            updateStatusAction("Scheduling jobs");

            List<string> jobIdsToWaitFor = new List<string>();

            foreach (var task in tasks)
            {
                _log.Info("Scheduling recurring job " + task.GetType().Name);

                RecurringJob.AddOrUpdate(task.JobId, () => task.Execute(JobCancellationToken.Null), task.Cron);

                if (task.TriggerOnRegister)
                {
                    var jobId = BackgroundJob.Enqueue(() => task.Execute(JobCancellationToken.Null));
                    jobIdsToWaitFor.Add(jobId);
                }
            }

            updateStatusAction("Waiting for startup jobs to complete");

            var monitorApi = JobStorage.Current.GetMonitoringApi();

            foreach (var jobIdToWaitFor in jobIdsToWaitFor)
            {
                WaitForEnqueuedTaskToComplete(monitorApi, jobIdToWaitFor);
            }
        }

        private void WaitForEnqueuedTaskToComplete(IMonitoringApi monitorApi, string jobIdToWaitFor)
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

            _log.Info("Waiting for the " + job.Job.Type.Name + " to complete");

            if (job.History.Any())
            {
                _log.Info("The " + job.Job.Type.Name + " job state is " + job.History[0].StateName);
            }

            Thread.Sleep(2000);

            WaitForEnqueuedTaskToComplete(monitorApi, jobIdToWaitFor);
        }
    }

    public interface IJobServer
    {
        BackgroundJobServerOptions BackgroundJobServerOptions { get; }
        void Start(Action<string> updateStatusAction);
        void Stop(Action<string> updateStatusAction);
    }
}
