using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuFridge.Service.Logging;
using NuFridge.Service.Model;
using NuFridge.Service.Repositories;

namespace NuFridge.Service.Feeds
{
    public sealed class FeedManager : IDisposable
    {
        private static FeedManager _instance;

        protected FeedManager()
        {
            FeedServices = new List<NuGetFeed>();
        }

        public static FeedManager Instance()
        {
            if (_instance == null)
            {
                _instance = new FeedManager();
            }

            return _instance;
        }

        private static readonly ILog Logger = LogProvider.For<FeedManager>(); 

        private List<NuGetFeed> FeedServices { get; set; }

        public IEnumerable<NuGetFeed> RunningFeeds {get {return FeedServices;}} 

        public void Dispose()
        {
            if (FeedServices.Any())
            {
                Logger.Info("Stopping NuGet feeds.");

                foreach (var feedService in FeedServices)
                {
                    feedService.Stop();
                }
            }
        }

        public bool Start(Feed feed)
        {
            var config = new ServiceConfiguration();

            NuGetFeed feedService = new NuGetFeed();

            if (!feedService.Start(feed))
            {
                Logger.Error("Failed to start " + feed.Name + ".");
                return false;
            }

            FeedServices.Add(feedService);

            Logger.Info("Successfully started " + feed.Name + ".");
            return true;
        }

        public bool Stop(Feed feed)
        {
            var service = FeedServices.FirstOrDefault(fd => fd.Id == feed.Id);
            if (service == null)
            {
                return false;
            }

            Logger.Info("Stopping " + feed.Name + ".");

            service.Stop();

            FeedServices.Remove(service);

            return true;
        }


        public void StartAll(ServiceConfiguration config)
        {
            Logger.Info("Starting NuGet feeds.");

            IRepository<Feed> feedRepository = new SqlCompactRepository<Feed>();

            var feeds = feedRepository.GetAll();

            bool success = true;

            if (!feeds.Any())
            {
                Logger.Info("No feeds found to start.");
            }
            else
            {
                Parallel.ForEach(feeds, feedEntity =>
                {
                    NuGetFeed feedService = new NuGetFeed();

                    if (!feedService.Start(feedEntity))
                    {
                        success = false;
                        Logger.Error("Failed to start " + feedEntity.Name + ".");
                        return;
                    }



                    Logger.Info("Successfully started " + feedEntity.Name + ".");

                    FeedServices.Add(feedService);
                });
            }

            if (!success)
            {
                Logger.Warn("Not all feeds have successfully been started.");
            }
        }
    }
}