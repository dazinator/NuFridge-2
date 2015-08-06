using Hangfire;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("background")]
    public class UpdateFeedPackageCountStatisticJob : JobBase
    {
        private readonly IFeedService _feedService;
        private readonly IPackageService _packageService;
        private readonly ILog _logger = LogProvider.For<UpdateFeedPackageCountStatisticJob>();

        public UpdateFeedPackageCountStatisticJob(IFeedService feedService, IPackageService packageService)
        {
            _feedService = feedService;
            _packageService = packageService;
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _logger.Info("Executing " + JobId + " job");

            FeedPackageCountStatistic stat = new FeedPackageCountStatistic(_feedService, _packageService);

            stat.UpdateModel(cancellationToken);
        }

        public override string JobId => typeof(UpdateFeedPackageCountStatisticJob).Name;

        public override string Cron => "*/10 * * * *"; //Every 10 minutes
    }
}