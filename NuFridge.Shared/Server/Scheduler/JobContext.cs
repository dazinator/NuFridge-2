using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Server;

namespace NuFridge.Shared.Server.Scheduler
{
    public class JobContext : IServerFilter
    {
        [ThreadStatic]
        private static string _jobId;

        public static string JobId { get { return _jobId; } set { _jobId = value; } }

        public void OnPerforming(PerformingContext context)
        {
            JobId = context.JobId;
        }

        public void OnPerformed(PerformedContext filterContext)
        {

        }
    }
}