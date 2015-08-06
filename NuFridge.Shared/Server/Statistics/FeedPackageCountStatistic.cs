using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics.Design;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedPackageCountStatistic : StatisticBase<List<FeedPackageCountStatisticItem>>
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public FeedPackageCountStatistic(IFeedService feedService, IPackageService packageService)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        protected override List<FeedPackageCountStatisticItem> Update()
        {
            var list = new List<FeedPackageCountStatisticItem>();

            var feeds = _feedService.GetAll();

            ColorGenerator generator = new ColorGenerator();


            foreach (var feed in feeds)
            {
                var packageCount = _packageService.GetUniquePackageIdCount(feed.Id);

                if (packageCount > 0)
                {
                    list.Add(new FeedPackageCountStatisticItem(feed.Name, packageCount, generator.NextColour()));
                }
            }


            var orderedList = list.OrderByDescending(it => it.PackageCount).ToList();
            if (orderedList.Count() > 10)
            {
                orderedList = orderedList.Take(10).ToList();
            }

            return orderedList;
        }

        protected override string StatName => "FeedPackageCount";
    }
}