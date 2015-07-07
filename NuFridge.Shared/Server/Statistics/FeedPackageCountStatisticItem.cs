using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedPackageCountStatisticItem
    {
        public string FeedName { get; set; }
        public int PackageCount { get; set; }
        public string Color { get; set; }

        public FeedPackageCountStatisticItem()
        {

        }

        public FeedPackageCountStatisticItem(string feedName, int packageCount, string color)
        {
            FeedName = feedName;
            PackageCount = packageCount;
            Color = color;
        }
    }
}
