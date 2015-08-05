using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server.Statistics.Design;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedDownloadCountStatistic : StatisticBase<List<FeedDownloadCountStatisticItem>>
    {
        protected override List<FeedDownloadCountStatisticItem> Update()
        {
            var list = new List<FeedDownloadCountStatisticItem>();

            using (var dbContext = new DatabaseContext())
            {
                var feeds = dbContext.Feeds.AsNoTracking();

                ColorGenerator generator = new ColorGenerator();

                foreach (var feed in feeds)
                {
                    var packages = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id).AsNoTracking().Where(pk => pk.FeedId == feed.Id);

                    if (packages.Any(pk => pk.VersionDownloadCount > 0))
                    {
                        list.Add(new FeedDownloadCountStatisticItem(feed.Name,
                            packages.Sum(pk => pk.VersionDownloadCount), generator.NextColour()));
                    }
                }

                var orderedList = list.OrderByDescending(it => it.DownloadCount).ToList();
                if (orderedList.Count() > 10)
                {
                    orderedList = orderedList.Take(10).ToList();
                }

                return orderedList;
            }
        }

        protected override string StatName
        {
            get { return "FeedDownloadCount"; }
        }
    }
}