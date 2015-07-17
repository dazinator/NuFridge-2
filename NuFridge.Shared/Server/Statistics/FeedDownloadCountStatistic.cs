using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Statistics.Design;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedDownloadCountStatistic : StatisticBase<List<FeedDownloadCountStatisticItem>>
    {
        public FeedDownloadCountStatistic(ITransaction transaction) : base(transaction)
        {

        }

        protected override List<FeedDownloadCountStatisticItem> Update()
        {
            var list = new List<FeedDownloadCountStatisticItem>();

            var feeds = Transaction.Query<IFeed>().ToList();

            ColorGenerator generator = new ColorGenerator();

            foreach (var feed in feeds)
            {
                var packages = Transaction.Query<IInternalPackage>().Where("FeedId = @feedId").Parameter("feedId", feed.Id).ToList();

                if (packages.Any(pk => pk.VersionDownloadCount > 0))
                {
                    list.Add(new FeedDownloadCountStatisticItem(feed.Name, packages.Sum(pk => pk.VersionDownloadCount), generator.NextColour()));
                }
            }

            return list.OrderByDescending(it => it.DownloadCount).ToList();
        }

        protected override string StatName
        {
            get { return "FeedDownloadCount"; }
        }
    }
}