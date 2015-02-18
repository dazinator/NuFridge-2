using System;
using System.Collections.Generic;
using System.Linq;
using NuFridge.Service.Data.Model;
using NuFridge.Service.Data.Repositories;

namespace NuFridge.Service.Feeds
{
    public sealed class FeedManager : IDisposable
    {
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
            Console.WriteLine("Starting NuGet feeds.");

            IRepository<Feed> feedRepository = new SqlCompactRepository<Feed>();

            var feeds = feedRepository.GetAll();

            bool success = true;

            if (!feeds.Any())
            {
                Console.WriteLine("No feeds found to start.");
            }
            else
            {
                foreach (var feedEntity in feeds)
                {
                    NuGetFeed feedService = new NuGetFeed(config);

                    if (!feedService.Start(feedEntity))
                    {
                        success = false;
                        Console.WriteLine("Failed to start " + feedEntity.Name);
                        continue;
                    }

                    Console.WriteLine("Successfully started " + feedEntity.Name);

                    FeedServices.Add(feedService);
                    break;
                }
            }

            if (!success)
            {
                Console.WriteLine("Warning: Not all feeds have successfully been started.");
            }
        }
    }
}