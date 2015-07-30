using Hangfire;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("background")]
    public class UpdateFeedPackageCountStatisticJob : JobBase
    {
        private IStore Store { get; set; }
        private readonly ILog _logger = LogProvider.For<UpdateFeedPackageCountStatisticJob>();

        public UpdateFeedPackageCountStatisticJob(IStore store) 
        {
            Store = store;
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetryAttribute(Attempts = 0)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _logger.Info("Executing " + JobId + " job");

            using (ITransaction transaction = Store.BeginTransaction())
            {
                FeedPackageCountStatistic stat = new FeedPackageCountStatistic(transaction);

                stat.UpdateModel(cancellationToken);
            }
        }

        public override string JobId => typeof(UpdateFeedPackageCountStatisticJob).Name;

        public override string Cron => "*/10 * * * *"; //Every 10 minutes
    }
}