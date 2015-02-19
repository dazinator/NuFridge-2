using System;
using System.ServiceProcess;
using NuFridge.Service.Api;
using NuFridge.Service.Data.Repositories;
using NuFridge.Service.Feeds;
using NuFridge.Service.Logging;

namespace NuFridge.Service
{
    public class Program
    {
        private static readonly ILog Logger = LogProvider.For<Program>(); 

        private static WebApiManager WebApiManager { get; set; }
        private static FeedManager FeedManager { get; set; }

        public const string ServiceName = "NuFridge Service";

        public Program()
        {
            WebApiManager = new WebApiManager();
            FeedManager = new FeedManager();
        }

        static void Main(string[] args)
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

        static bool ValidateConfig(ServiceConfiguration config)
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

        public static bool MigrateDatabase()
        {
            Logger.Info("Executing database migrations.");

            using (var context = new NuFridgeContext())
            {
                return context.Upgrade();
            }
        }

        public static void Start(string[] args)
        {
            Logger.Info("NuFridge Service");

            var config = new ServiceConfiguration();

            if (!ValidateConfig(config))
            {
                return;
            }

            if (!MigrateDatabase())
            {
                return;
            }

            WebApiManager = new WebApiManager();
            WebApiManager.Start(config);

            FeedManager = new FeedManager();
            FeedManager.Start(config);
        }

        public static void Stop()
        {
            if (WebApiManager != null)
            {
                WebApiManager.Dispose();
            }

            if (FeedManager != null)
            {
                FeedManager.Dispose();
            }
        }
    }
}