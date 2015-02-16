using System;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.ServiceProcess;
using NuFridge.Common;
using NuFridge.Service.Api;

namespace NuFridge.Service
{
    public class Service : ServiceBase
    {
        public Service()
        {
            ServiceName = Program.ServiceName;
        }

        protected override void OnStart(string[] args)
        {
            Program.Start(args);
        }

        protected override void OnStop()
        {
            Program.Stop();
        }
    }

    public class Program
    {
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

                Console.WriteLine("Press the <enter> key to quit.");

                Console.ReadLine();

                Stop();
            }
        }

        static bool ValidateConfig(ServiceConfiguration config)
        {
            Console.WriteLine("Validating config file.");

            var result = config.Validate();

            if (!result.Success)
            {
                Console.WriteLine("The config file is not valid.");

                if (result.Exception != null)
                {
                    Console.WriteLine("Error message: " + result.Exception.Message);
                }
                return false;
            }

            return true;
        }

        

        public static void Start(string[] args)
        {
            Console.WriteLine("NuFridge Service");

            var config = new ServiceConfiguration();

            if (!ValidateConfig(config))
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