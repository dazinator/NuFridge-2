using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Scheduler;
using NuFridge.Shared.Server.Scheduler.Jobs;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web;

namespace NuFridge.Shared.Server
{
    public class ServerEngine : IServerEngine
    {
        private readonly Lazy<IWebServerInitializer> _webHostInitializer;
        private readonly Lazy<IStoreInitializer> _storeInitializer;
        private BackgroundJobServer _backgroundJobServer;
        public BackgroundJobServerOptions BackgroundJobServerOptions { get; private set; }
        private readonly IStore _store;
        private readonly IContainer _container;
        private readonly ILog _log = LogProvider.For<ServerEngine>();

        public ServerEngine(IContainer container, Lazy<IWebServerInitializer> webHostInitializer, Lazy<IStoreInitializer> storeInitializer, IStore store)
        {
            _webHostInitializer = webHostInitializer;
            _storeInitializer = storeInitializer;
            _store = store;
            _container = container;
        }

        public void Start()
        {
            _storeInitializer.Value.Initialize();


            _log.Info("Starting job server.");

            var options = new SqlServerStorageOptions() {PrepareSchemaIfNecessary = true};
            GlobalConfiguration.Configuration.UseSqlServerStorage(_store.ConnectionString, options);
            GlobalConfiguration.Configuration.UseAutofacActivator(_container);
            HangfirePerLifetimeScopeConfigurer.Configure(_container);
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute {Attempts = 0});
            BackgroundJobServerOptions = new BackgroundJobServerOptions();

            _backgroundJobServer = new BackgroundJobServer(BackgroundJobServerOptions);

            _log.Info("Successfully started job server.");

            var tasks = _container.Resolve<IEnumerable<JobBase>>().ToList();

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

            var monitorApi = JobStorage.Current.GetMonitoringApi();

            //This slows down the startup of the program which makes quick changes in debug more tedious
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                foreach (var jobIdToWaitFor in jobIdsToWaitFor)
                {
                    WaitForEnqueuedTaskToComplete(monitorApi, jobIdToWaitFor);
                }
            }


            IWebServerInitializer webSserverInitializer = _webHostInitializer.Value;
            webSserverInitializer.Starting("Starting the web server");
            webSserverInitializer.Start();
            webSserverInitializer.Started();
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


        public void Stop()
        {
            _webHostInitializer.Value.Stop();

            _backgroundJobServer.Dispose();
        }
    }
}
