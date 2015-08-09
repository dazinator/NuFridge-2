using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using NuFridge.Shared.Server.NuGet;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("filesystem")]
    public class CleanupExpiredImportPackagesJob : JobBase
    {
        public override string JobId => typeof(CleanupExpiredImportPackagesJob).Name;
        public override bool TriggerOnRegister => false;
        public override string Cron => Hangfire.Cron.Daily(20);

        public override void Execute(IJobCancellationToken cancellationToken)
        {
            PackageImportProgressTracker.Instance.RemoveExpiredJobs();
        }
    }
}