using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuFridge.Shared.Reporting
{
    public class ReportGraph
    {
        public ReportGraph(Dictionary<string, long> feedPackageCount)
        {
            Series.Add(new List<long>());

            foreach (var item in feedPackageCount)
            {
                Labels.Add(item.Key);
                Series[0].Add(item.Value);
            }
        }

        public ReportGraph()
        {

        }

        [JsonProperty("labels")]
        public List<string> Labels = new List<string>();

        [JsonProperty("series")]
        public List<List<long>> Series = new List<List<long>>();
    }
}
