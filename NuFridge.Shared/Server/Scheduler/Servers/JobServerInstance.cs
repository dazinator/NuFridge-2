using System;
using Hangfire;
using Hangfire.Storage;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Scheduler.Servers
{
    public abstract class JobServerInstance
    {
        protected readonly ILog Log = LogProvider.For<JobServerInstance>();

        private BackgroundJobServer _backgroundJobServer;

        public BackgroundJobServerOptions BackgroundJobServerOptions { get; private set; }

        public abstract int WorkerCount { get;  }
        public abstract string QueueName { get; }

        public virtual void BeforeStart(IMonitoringApi monitorApi, Action<string> updateStatusAction)
        {
            
        }

        public virtual void AfterStart()
        {
            
        }

        public void Start(IMonitoringApi monitorApi, Action<string> updateStatusAction)
        {
            BeforeStart(monitorApi, updateStatusAction);

            BackgroundJobServerOptions = new BackgroundJobServerOptions
            {
                Queues = new[] { QueueName },
                WorkerCount = WorkerCount,
                ServerName = $"{Environment.MachineName}-{QueueName}"
            };

            _backgroundJobServer = new BackgroundJobServer(BackgroundJobServerOptions);

            Log.Info("Successfully started job server");

            AfterStart();
        }

        public void Stop()
        {
            _backgroundJobServer.Dispose();
        }
    }
}