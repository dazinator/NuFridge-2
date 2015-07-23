using Hangfire;
using Hangfire.Logging;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class ReindexPackagesForFeedJob : JobBase
    {
        private readonly IStore _store;
        private readonly ILog _logger = LogProvider.For<ReindexPackagesForFeedJob>();

        public ReindexPackagesForFeedJob(IStore store)
        {
            _store = store;
        }

        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _logger.Info("Executing " + JobId + " job");

            cancellationToken.ThrowIfCancellationRequested();
        }

        public override string JobId => typeof (ReindexPackagesForFeedJob).Name;
        public override bool IsRecurring => false;
    }
}