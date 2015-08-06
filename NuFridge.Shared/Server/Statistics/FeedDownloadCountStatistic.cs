using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics.Design;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedDownloadCountStatistic : StatisticBase<List<FeedDownloadCountStatisticItem>>
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public FeedDownloadCountStatistic(IFeedService feedService, IPackageService packageService)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        protected override List<FeedDownloadCountStatisticItem> Update()
        {
            var list = new List<FeedDownloadCountStatisticItem>();


            var feeds = _feedService.GetAll();

            ColorGenerator generator = new ColorGenerator();

            foreach (var feed in feeds)
            {
                IEnumerable<IInternalPackage> packages = _packageService.GetAllPackagesForFeed(feed.Id).ToList();

                if (packages.Any(pk => pk.VersionDownloadCount > 0))
                {
                    list.Add(new FeedDownloadCountStatisticItem(feed.Name, packages.Sum(pk => pk.VersionDownloadCount), generator.NextColour()));
                }
            }

            var orderedList = list.OrderByDescending(it => it.DownloadCount).ToList();
            if (orderedList.Count() > 10)
            {
                orderedList = orderedList.Take(10).ToList();
            }

            return orderedList;

        }

        protected override string StatName => "FeedDownloadCount";
    }
}