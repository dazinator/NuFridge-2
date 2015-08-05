using Hangfire;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("background")]
    public class UpdateFeedDownloadCountStatisticJob : JobBase
    {
        private IStore Store { get; set; }
        private readonly ILog _logger = LogProvider.For<UpdateFeedDownloadCountStatisticJob>();

        public UpdateFeedDownloadCountStatisticJob(IStore store)
        {
            Store = store;
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _logger.Info("Executing " + JobId + " job");

                FeedDownloadCountStatistic stat = new FeedDownloadCountStatistic();

                stat.UpdateModel(cancellationToken);
            
        }

        public override string JobId => typeof(UpdateFeedDownloadCountStatisticJob).Name;

        public override string Cron => "*/10 * * * *"; //Every 10 minutes
    }
}