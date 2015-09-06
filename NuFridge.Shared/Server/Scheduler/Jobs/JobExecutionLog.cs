using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class JobExecutionLog
    {
        public List<JobExecutionLogItem> Items = new List<JobExecutionLogItem>();

        public class JobExecutionLogItem
        {
            public string Message { get; set; }

            [JsonConverter(typeof(StringEnumConverter))]
            public LogLevel Level { get; set; }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}