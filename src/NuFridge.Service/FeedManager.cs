using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Common;
using NuFridge.DataAccess.Repositories;

namespace NuFridge.Service
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
            IRepository<DataAccess.Model.Feed> feedRepository = new SqlCompactRepository<DataAccess.Model.Feed>();

            Console.WriteLine("Getting a list of feeds.");

            var feeds = feedRepository.GetAll();

            foreach (var feedEntity in feeds)
            {
                NuGetFeed feedService = new NuGetFeed(config);

                feedService.Start(feedEntity);

                FeedServices.Add(feedService);
                break;
            }
        }
    }
}