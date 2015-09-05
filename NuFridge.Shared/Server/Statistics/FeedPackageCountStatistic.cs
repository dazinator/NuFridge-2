using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics.Design;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedPackageCountStatistic : StatisticBase<FeedPackageCountStatisticItem>
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public FeedPackageCountStatistic(IFeedService feedService, IPackageService packageService, IStatisticService statisticService) : base(statisticService)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        protected override FeedPackageCountStatisticItem Update()
        {
            var feeds = _feedService.GetAll();

            Dictionary<string, long> feedPackageCount = new Dictionary<string, long>();


            foreach (var feed in feeds)
            {
                long packageCount = _packageService.GetUniquePackageIdCount(feed.Id);

                feedPackageCount.Add(feed.Name, packageCount);
            }

            return new FeedPackageCountStatisticItem(feedPackageCount);
        }

        protected override string StatName => "FeedPackageCount";
    }
}