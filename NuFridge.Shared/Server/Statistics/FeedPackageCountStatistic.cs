using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server.Statistics.Design;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedPackageCountStatistic : StatisticBase<List<FeedPackageCountStatisticItem>>
    {
        private readonly IStore _store;

        public FeedPackageCountStatistic(IStore store)
        {
            _store = store;
        }

        protected override List<FeedPackageCountStatisticItem> Update()
        {
            var list = new List<FeedPackageCountStatisticItem>();

            using (var dbContext = new DatabaseContext())
            {
                var feeds = dbContext.Feeds.AsNoTracking();

                ColorGenerator generator = new ColorGenerator();


                foreach (var feed in feeds)
                {
                    var packageCount =
                        EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id)
                            .Count(pk => pk.IsLatestVersion || pk.IsAbsoluteLatestVersion);

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
        }

        protected override string StatName
        {
            get { return "FeedPackageCount"; }
        }
    }
}