using System;
using Hangfire;
using Hangfire.Storage;

namespace NuFridge.Shared.Scheduler.Servers
{
    public class JobServerBackground : JobServerInstance
    {
        public override string QueueName => "background";

        public override int WorkerCount => Math.Min(Environment.ProcessorCount * 3, 24);

        public override void BeforeStart(IMonitoringApi monitorApi, Action<string> updateStatusAction)
        {
            RecurringJob.RemoveIfExists("UpdateSystemInformationJob");
            RecurringJob.RemoveIfExists("UpdateFeedDownloadCountStatisticJob");
            RecurringJob.RemoveIfExists("UpdateFeedPackageCountStatisticJob");
        }
    }
}