using System;
using System.Linq;
using System.Threading;
using FluentScheduler;
using FluentScheduler.Model;
using NuFridge.Service.Logging;
using NuFridge.Service.Scheduler.Tasks;

namespace NuFridge.Service.Scheduler
{
    public class TaskScheduler : Registry, IDisposable
    {
        private static TaskScheduler _instance;

        protected TaskScheduler()
        {
            Schedule<PackageRepositoryCleaner>().ToRunNow().AndEvery(1).Days().At(11, 00);
        }

        public static TaskScheduler Instance()
        {
            if (_instance == null)
            {
                _instance = new TaskScheduler();
            }

            return _instance;
        }

        private static readonly ILog Logger = LogProvider.For<Program>();

        public void Start()
        {
            TaskManager.UnobservedTaskException += Exception;
            TaskManager.Initialize(this);
        }

        public void Exception(TaskExceptionInformation sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log(LogLevel.Error, () => sender.Name + ": Failed with an uncaught exception. " + sender.Task.Exception);
        }

        public void Dispose()
        {
            Logger.Info("Stopping scheduled tasks.");

            TaskManager.Stop();

            int numberWaitingFor = 0;

            while (TaskManager.RunningSchedules.Any())
            {
                if (numberWaitingFor != TaskManager.RunningSchedules.Count())
                {
                    numberWaitingFor = TaskManager.RunningSchedules.Count();
                    Logger.Info(string.Format("Waiting for {0} scheduled tasks to finish.", numberWaitingFor));
                }
                Thread.Sleep(500);
            }
        }
    }
}