using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Reporting
{
    public class FeedDownloadCountReport
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;

        public FeedDownloadCountReport(IFeedService feedService, IPackageService packageService)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        public ReportGraph GetModel()
        {
            var feeds = _feedService.GetAll();

            Dictionary<string, long> feedPackageCount = new Dictionary<string, long>();


            foreach (var feed in feeds)
            {
                long packageCount = _packageService.GetPackageDownloadCount(feed.Id);

                feedPackageCount.Add(feed.Name, packageCount);
            }

            if (feedPackageCount.Count > 10)
            {
                feedPackageCount = feedPackageCount.OrderByDescending(pc => pc.Value).Take(10).ToDictionary(x => x.Key, x => x.Value);
            }

            return new ReportGraph(feedPackageCount);
        }
    }
}