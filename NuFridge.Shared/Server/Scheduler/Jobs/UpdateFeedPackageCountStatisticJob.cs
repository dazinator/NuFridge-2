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
        private readonly IStatisticService _statisticService;
        private readonly ILog _logger = LogProvider.For<UpdateFeedPackageCountStatisticJob>();

        public UpdateFeedPackageCountStatisticJob(IFeedService feedService, IPackageService packageService, IStatisticService statisticService)
        {
            _feedService = feedService;
            _packageService = packageService;
            _statisticService = statisticService;
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _logger.Info("Executing " + JobId + " job");

            FeedPackageCountStatistic stat = new FeedPackageCountStatistic(_feedService, _packageService, _statisticService);

            stat.UpdateModel(cancellationToken);
        }

        public override string JobId => typeof(UpdateFeedPackageCountStatisticJob).Name;

        public override string Cron => "*/10 * * * *"; //Every 10 minutes
    }
}