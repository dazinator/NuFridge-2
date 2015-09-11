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

            Dictionary<string, long> feedDownloadCount = new Dictionary<string, long>();

            foreach (var feed in feeds)
            {
                IEnumerable<IInternalPackage> packages = _packageService.GetAllPackagesForFeed(feed.Id).ToList();

                if (packages.Any(pk => pk.VersionDownloadCount > 0))
                {
                    feedDownloadCount.Add(feed.Name, packages.Sum(pk => pk.VersionDownloadCount));
                }
            }

            if (feedDownloadCount.Count > 10)
            {
                feedDownloadCount = feedDownloadCount.OrderByDescending(pc => pc.Value).Take(10).ToDictionary(x => x.Key, x => x.Value);
            }

            return new ReportGraph(feedDownloadCount);
        }

        //protected override string StatName => "FeedDownloadCount";
    }
}