using System;
using System.Net;
using System.ServiceProcess;
using AutoMapper;
using FluentScheduler;
using NuFridge.Service.Feeds;
using NuFridge.Service.Logging;
using NuFridge.Service.Model;
using NuFridge.Service.Model.Dto;
using NuFridge.Service.Repositories;
using NuFridge.Service.Scheduler;
using NuFridge.Service.Website;

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

        private static void TryValidateConfig(ServiceConfiguration config)
        {
            Logger.Info("Validating config file.");

            var result = config.Validate();

            if (!result.Success)
            {
                var message = "The config file is not valid.";

                Logger.Error(message);



                if (result.Exception != null)
                {
                    message = result.Exception.Message;
                    Logger.Error("Error message: " + message);
                }
                throw new Exception(message);
            }
        }


        public static void Start(string[] args)
        {
            Logger.Info("NuFridge Service");

            var config = new ServiceConfiguration();

            try
            {
                TryValidateConfig(config);
            }
            catch (Exception ex)
            {
                ProcessStartupException(ex);
                Stop();
                throw;
            }

            try
            {
                NuFridgeContext.TryUpgrade();
            }
            catch (Exception ex)
            {
                ProcessStartupException(ex);
                Stop();
                throw;
            }

            try
            {
                WebsiteManager.Instance().Start(config);
            }
            catch (Exception ex)
            {
                ProcessStartupException(ex);
                Stop();
                throw;
            }

            Mapper.CreateMap<ApplicationUser, DtoApplicationUser>()
    .ForMember(x => x.Email, o => o.MapFrom(s => s.Email))
    .ForMember(x => x.EmailConfirmed, o => o.MapFrom(s => s.EmailConfirmed))
    .ForMember(x => x.FirstName, o => o.MapFrom(s => s.FirstName))
    .ForMember(x => x.Id, o => o.MapFrom(s => s.Id))
    .ForMember(x => x.LastName, o => o.MapFrom(s => s.LastName))
    .ForMember(x => x.UserName, o => o.MapFrom(s => s.UserName));

            try
            {
                FeedManager.Instance().StartAll(config);
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
            WebsiteManager.Instance().Dispose();
            FeedManager.Instance().Dispose();

            TaskScheduler.Instance().Dispose();
        }
    }
}