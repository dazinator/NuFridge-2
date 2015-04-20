using System;
using System.Collections.Generic;
using System.Linq;
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
                    feedService.Dispose();
                }
            }
        }


        public void Start(ServiceConfiguration config)
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
                foreach (var feedEntity in feeds)
                {
                    NuGetFeed feedService = new NuGetFeed(config);

                    if (!feedService.Start(feedEntity))
                    {
                        success = false;
                        Logger.Error("Failed to start " + feedEntity.Name + ".");
                        continue;
                    }

                    Logger.Info("Successfully started " + feedService.BaseAddress + ".");

                    FeedServices.Add(feedService);
                }
            }

            if (!success)
            {
                Logger.Warn("Not all feeds have successfully been started.");
            }
        }
    }
}