using System;
using System.Net;
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

            if (!NuFridgeContext.TryUpgrade())
            {
                return;
            }

            try
            {
                WebApiManager.Instance().Start(config);
            }
            catch (Exception ex)
            {
                ProcessStartupException(ex);
                Stop();
                throw;
            }

            try
            {
                FeedManager.Instance().Start(config);
            }
            catch (Exception ex)
            {
                ProcessStartupException(ex);
                Stop();
                throw;
            }

            try
            {
                TaskScheduler.Instance().Start();
            }
            catch (Exception ex)
            {
                ProcessStartupException(ex);
                Stop();
                throw;
            }
        }

        public static void ProcessStartupException(Exception ex)
        {
            var baseException = ex.GetBaseException();

            Logger.Error("There was an error starting NuFridge.");

            if (baseException is HttpListenerException)
            {
                if (baseException.Message.ToLower().StartsWith("the process cannot access the file because it is being used by another process"))
                {
                    Logger.Error(string.Format("A port specified in the config file is already in use."));
                }
                else
                {
                    Logger.Error(baseException.Message + "\r\n" + ex.StackTrace);
                }
            }
            else
            {
                Logger.Error("There was an unexpected error starting NuFridge.");
                Logger.Error(baseException.Message + "\r\n" + ex.StackTrace);
            }
        }


        public static void Stop()
        {
            WebApiManager.Instance().Dispose();
            FeedManager.Instance().Dispose();

            TaskScheduler.Instance().Dispose();
        }
    }
}