using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuFridge.Shared.Server.Statistics
{
    public class FeedPackageCountStatisticItem
    {
        public FeedPackageCountStatisticItem(Dictionary<string, long> feedPackageCount)
        {
            Series.Add(new List<long>());

            foreach (var item in feedPackageCount)
            {
                Labels.Add(item.Key);
                Series[0].Add(item.Value);
            }
        }

        public FeedPackageCountStatisticItem()
        {
            
        }

        [JsonProperty("labels")]
        public List<string> Labels = new List<string>();

        [JsonProperty("series")]
        public List<List<long>> Series = new List<List<long>>();
    }
}