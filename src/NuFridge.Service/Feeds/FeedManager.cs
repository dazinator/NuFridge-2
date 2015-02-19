using System;
using System.Collections.Generic;
using System.Linq;
using NuFridge.Service.Data.Model;
using NuFridge.Service.Data.Repositories;
using NuFridge.Service.Logging;

namespace NuFridge.Service.Feeds
{
    public sealed class FeedManager : IDisposable
    {
        private static readonly ILog Logger = LogProvider.For<FeedManager>(); 

        List<NuGetFeed> FeedServices { get; set; }

        public FeedManager()
        {
            FeedServices = new List<NuGetFeed>();
        }

        public void Dispose()
        {
            foreach (var feedService in FeedServices)
            {
                feedService.Dispose();
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
                        Logger.Info("Failed to start " + feedEntity.Name + ".");
                        continue;
                    }

                    Logger.Info("Successfully started " + feedEntity.Name + ".");

                    FeedServices.Add(feedService);
                    break;
                }
            }

            if (!success)
            {
                Logger.Info("Warning: Not all feeds have successfully been started.");
            }
        }
    }
}