using System;
using System.Linq;
using Hangfire;
using Hangfire.Storage;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Scheduler.Servers
{
    public class JobServerFileSystem : JobServerInstance
    {
        public override string QueueName => "filesystem";

        public override int WorkerCount => Math.Min(Environment.ProcessorCount * 3, 24);

        public override void BeforeStart(IMonitoringApi monitorApi, Action<string> updateStatusAction)
        {
            var queuedJobs = monitorApi.EnqueuedJobs(QueueName, 0, int.MaxValue).ToList();

            if (queuedJobs.Any())
            {
                Log.Warn("Deleting " + queuedJobs.Count() + " jobs which are currently queued.");

                updateStatusAction("Deleting " + queuedJobs.Count() + " jobs which are currently queued.");

                foreach (var queuedJob in queuedJobs)
                {
                    BackgroundJob.Delete(queuedJob.Key);
                }
            }

            var fetchedJobs =
                monitorApi.FetchedJobs(QueueName, 0, int.MaxValue)
                    .ToList();


            if (fetchedJobs.Any())
            {
                Log.Warn("Deleting " + fetchedJobs.Count() + " jobs which are currently feteched.");

                updateStatusAction("Deleting " + fetchedJobs.Count() + " jobs which are currently fetched.");

                foreach (var fetchedJob in fetchedJobs)
                {
                    BackgroundJob.Delete(fetchedJob.Key);
                }
            }
        }
    }
}