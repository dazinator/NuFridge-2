using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using NCrontab;
using NuFridge.Shared.Autofac;
using NuFridge.Shared.Database;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Scheduler.Jobs;
using NuFridge.Shared.Scheduler.Jobs.Definitions;
using NuFridge.Shared.Scheduler.Servers;

namespace NuFridge.Shared.Scheduler
{
    public class JobServerManager : IJobServerManager
    {
        private readonly ILog _log = LogProvider.For<JobServerManager>();
        private readonly IStore _store;
        private readonly IEnumerable<JobServerInstance> _jobServerInstances;
        private readonly IContainer _container;

        public JobServerManager(IContainer container, IStore store, IEnumerable<JobServerInstance> jobServerInstances)
        {
            _container = container;
            _store = store;
            _jobServerInstances = jobServerInstances;
        }

        public void Stop(Action<string> updateStatusAction)
        {
            _log.Info("Shutting down the job server.");

            var monitorApi = JobStorage.Current.GetMonitoringApi();

            var processingJobs = monitorApi.ProcessingJobs(0, 10);

            foreach (var processingJob in processingJobs)
            {
                _log.Warn("The " + processingJob.Value.Job.Type.Name + " job will be terminated due to the service shutting down. It will restart on the next service start.");
            }

            updateStatusAction("Shutting down the job server");

            foreach (var jobServerInstance in _jobServerInstances)
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
            GlobalConfiguration.Configuration.UseSqlServerStorage(_store.ConnectionString, options).UseMsmqQueues(@".\Private$\nufridge_{0}", "filesystem", "download");
            GlobalConfiguration.Configuration.UseAutofacActivator(_container);
            HangfirePerLifetimeScopeConfigurer.Configure(_container);
            GlobalConfiguration.Configuration.UseFilter(new JobContext());

            var monitorApi = JobStorage.Current.GetMonitoringApi();

            foreach (var jobServerInstance in _jobServerInstances)
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
        }
    }

    public interface IJobServerManager
    {
        void Start(Action<string> updateStatusAction);
        void Stop(Action<string> updateStatusAction);
    }
}