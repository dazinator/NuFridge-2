using Hangfire;
using Hangfire.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class ReindexPackagesForFeedJob
    {
        private readonly IStore _store;
        private readonly ILog _logger = LogProvider.For<ReindexPackagesForFeedJob>();

        public ReindexPackagesForFeedJob(IStore store)
        {
            _store = store;
        }

        public void Execute(IJobCancellationToken cancellationToken, int feedId)
        {
            _logger.Info("Executing " + JobId + " job for feed id " + feedId);

            cancellationToken.ThrowIfCancellationRequested();


        }

        public string JobId => typeof (ReindexPackagesForFeedJob).Name;
    }
}