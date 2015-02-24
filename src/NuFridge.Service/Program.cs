using System;
using System.ServiceProcess;
using FluentScheduler;
using NuFridge.Service.Api;
using NuFridge.Service.Data.Repositories;
using NuFridge.Service.Feeds;
using NuFridge.Service.Logging;
using NuFridge.Service.Scheduler;

namespace NuFridge.Service
{
    public class Program
    {
        private static readonly ILog Logger = LogProvider.For<Program>();
        public const string ServiceName = "NuFridge Service";

        private static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                using (var service = new Service())
                {
                    ServiceBase.Run(service);
                }
            }
            else
            {
                Start(args);

                Logger.Info("Press the <enter> key to quit.");
                Console.ReadLine();

                Stop();
            }
        }

        private static bool ValidateConfig(ServiceConfiguration config)
        {
            Logger.Info("Validating config file.");

            var result = config.Validate();

            if (!result.Success)
            {
                Logger.Info("The config file is not valid.");

                if (result.Exception != null)
                {
                    Logger.Info("Error message: " + result.Exception.Message);
                }
                return false;
            }

            return true;
        }


        public static void Start(string[] args)
        {
            Logger.Info("NuFridge Service");

            var config = new ServiceConfiguration();

            if (!ValidateConfig(config))
            {
                return;
            }

            if (!NuFridgeContext.Upgrade())
            {
                return;
            }

            WebApiManager.Instance().Start(config);
            FeedManager.Instance().Start(config);
            TaskScheduler.Instance().Start();
        }



        public static void Stop()
        {
            WebApiManager.Instance().Dispose();
            FeedManager.Instance().Dispose();

            TaskScheduler.Instance().Dispose();
        }
    }
}