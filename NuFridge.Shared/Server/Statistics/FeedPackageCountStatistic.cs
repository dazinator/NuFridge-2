using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Statistics.Design;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedPackageCountStatistic : StatisticBase<List<FeedPackageCountStatisticItem>>
    {
        private readonly IStore _store;

        public FeedPackageCountStatistic(ITransaction transaction, IStore store)
            : base(transaction)
        {
            _store = store;
        }

        protected override List<FeedPackageCountStatisticItem> Update()
        {
            var list = new List<FeedPackageCountStatisticItem>();

            var feeds = Transaction.Query<IFeed>().ToList();

            ColorGenerator generator = new ColorGenerator();

            using (var dbContext = new ReadOnlyDatabaseContext(_store))
            {
                foreach (var feed in feeds)
                {
                    var packageCount = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feed.Id).Count(pk => pk.IsLatestVersion || pk.IsAbsoluteLatestVersion);

                    if (packageCount > 0)
                    {
                        list.Add(new FeedPackageCountStatisticItem(feed.Name, packageCount, generator.NextColour()));
                    }
                }
            }

            var orderedList = list.OrderByDescending(it => it.PackageCount).ToList();
            if (orderedList.Count() > 10)
            {
                orderedList = orderedList.Take(10).ToList();
            }

            return orderedList;
        }

        protected override string StatName
        {
            get { return "FeedPackageCount"; }
        }
    }
}