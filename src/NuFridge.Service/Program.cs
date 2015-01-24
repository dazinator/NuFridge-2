using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Amib.Threading;
using NuFridge.DataAccess.Repositories;
using NuFridge.Service.Feed;

namespace NuFridge.Service
{


    public class Program
    {


        public const string ServiceName = "NuFridge Service";

        public class Service : ServiceBase
        {
            public Service()
            {
                ServiceName = Program.ServiceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start();
            }

            protected override void OnStop()
            {
                Program.Stop();
            }
        }


        static List<NuGetFeed> FeedServices { get; set; }
        static SmartThreadPool ThreadPool { get; set; }

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                // running as service
                using (var service = new Service())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                Start();

                Console.WriteLine("Press the <enter> key to quit.");

                Console.ReadLine();



                Stop();
            }

           
        }

        //TODO API Key
        static void Start()
        {
            Console.WriteLine("NuFridge Service");


            Console.WriteLine("Connecting to the database.");

            IRepository<DataAccess.Model.Feed> feedRepository = new SqlCompactRepository<DataAccess.Model.Feed>();

            Console.WriteLine("Getting a list of feeds.");

            var feeds = feedRepository.GetAll();

            var rootPath = @"C:\inetpub\wwwroot\NuFridge\Feeds\";

            ThreadPool = new SmartThreadPool();

            FeedServices = new List<NuGetFeed>();

            List<IWorkItemResult> disposeResults = new List<IWorkItemResult>();

            foreach (var feedEntity in feeds)
            {
                NuGetFeed feedService = new NuGetFeed();

                feedService.NewPackageDetected += delegate(object sender, EventArgs args)
                {
                    var t = 1;
                };

                var feedPath = rootPath + feedEntity.Name;

                var disposeResult = ThreadPool.QueueWorkItem<string, string, string>(feedService.Start, feedEntity.Name, feedPath, "http://*:82/Feeds/" + feedEntity.Name);

                FeedServices.Add(feedService);

                disposeResults.Add(disposeResult);
                break;
            }

        

            StartCheck(disposeResults);
        }

        static void Stop()
        {
            Dispose();
        }

        static void StartCheck(List<IWorkItemResult> disposeResults)
        {
            int total = disposeResults.Count();
            int remaining = total;

            foreach (var disposeResult in disposeResults)
            {
                if (disposeResult.IsCompleted || disposeResult.IsCanceled)
                {
                    remaining--;
                }
            }

            if (remaining != 0)
            {
                Thread.Sleep(500);
                StartCheck(disposeResults);
            } 
        }

        static void DisposeCheck(List<IWorkItemResult> disposeResults)
        {
            Console.WriteLine("Waiting for all feeds to stop.");

            int total = disposeResults.Count();
            int remaining = total;

            foreach (var disposeResult in disposeResults)
            {
                if (disposeResult.IsCompleted || disposeResult.IsCanceled)
                {
                    remaining--;
                }
            }

            if (remaining != 0)
            {
                Thread.Sleep(500);
                DisposeCheck(disposeResults);
            }
        }

        static void Dispose()
        {
            List<IWorkItemResult> disposeResults = new List<IWorkItemResult>();

            foreach (var feedService in FeedServices)
            {
                var disposeResult = ThreadPool.QueueWorkItem(feedService.Dispose);

                disposeResults.Add(disposeResult);
            }

            DisposeCheck(disposeResults);
        }
    }
}