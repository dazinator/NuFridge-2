﻿using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Statistics.Design;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedPackageCountStatistic : StatisticBase<List<FeedPackageCountStatisticItem>>
    {
        public FeedPackageCountStatistic(ITransaction transaction)
            : base(transaction)
        {

        }

        protected override List<FeedPackageCountStatisticItem> Update()
        {
            var list = new List<FeedPackageCountStatisticItem>();

            var feeds = Transaction.Query<IFeed>().ToList();

            ColorGenerator generator = new ColorGenerator();
           
            foreach (var feed in feeds)
            {
                var packageCount = Transaction.Query<IInternalPackage>().Where("FeedId = @feedId").Where("IsLatestVersion = 1 OR IsAbsoluteLatestVersion = 1").Parameter("feedId", feed.Id).Count();

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

        protected override string StatName
        {
            get { return "FeedPackageCount"; }
        }
    }
}