using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentScheduler;
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
        private readonly IContainer _container;
        private ILog _log = LogProvider.For<ServerEngine>();

        public ServerEngine(IContainer container, Lazy<IWebServerInitializer> webHostInitializer, Lazy<IStoreInitializer> storeInitializer)
        {
            _webHostInitializer = webHostInitializer;
            _storeInitializer = storeInitializer;
            _container = container;
        }

        void TaskManager_UnobservedTaskException(FluentScheduler.Model.TaskExceptionInformation sender, UnhandledExceptionEventArgs e)
        {
            _log.Fatal("An error happened with a scheduled task: " + e.ExceptionObject);
        }

        public void Start()
        {
            IWebServerInitializer webSserverInitializer = _webHostInitializer.Value;

            webSserverInitializer.Start();
            webSserverInitializer.Starting("Loading...");

            _storeInitializer.Value.Initialize();

            webSserverInitializer.Started();

            _log.Info("Starting scheduled tasks");

            var jobs = _container.Resolve<IEnumerable<IJob>>();

            _log.Info("Found " + jobs.Count() + " scheduled tasks.");

            TaskManager.TaskFactory = new AutofacTaskFactory(_container);
            TaskManager.UnobservedTaskException += TaskManager_UnobservedTaskException;
            TaskManager.Initialize(new SchedulerRegistry(jobs));
        }

        public void Stop()
        {
            _webHostInitializer.Value.Stop();
        }
    }
}
