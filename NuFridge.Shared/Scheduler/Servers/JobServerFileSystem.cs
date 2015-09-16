using System;
using Hangfire;
using Hangfire.Storage;

namespace NuFridge.Shared.Scheduler.Servers
{
    public class JobServerFileSystem : JobServerInstance
    {
        public override string QueueName => "filesystem";

        public override int WorkerCount => Math.Min(Environment.ProcessorCount * 3, 24);

        public override void BeforeStart(IMonitoringApi monitorApi, Action<string> updateStatusAction)
        {
            RecurringJob.RemoveIfExists("UpdateSystemInformationJob");
            RecurringJob.RemoveIfExists("UpdateFeedDownloadCountStatisticJob");
            RecurringJob.RemoveIfExists("UpdateFeedPackageCountStatisticJob");
            RecurringJob.RemoveIfExists("CleanupExpiredImportPackagesJob");
        }
    }
}