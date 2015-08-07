using Hangfire;
using Hangfire.Logging;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("background")]
    public class UpdateSystemInformationJob : JobBase
    {
        private readonly IJobServerManager _jobServerManager;
        private readonly IStatisticService _statisticService;
        private IStore Store { get; set; }
        private readonly ILog _logger = LogProvider.For<UpdateSystemInformationJob>();

        public UpdateSystemInformationJob(IStore store, IJobServerManager jobServerManager, IStatisticService statisticService) 
        {
            _jobServerManager = jobServerManager;
            _statisticService = statisticService;
            Store = store;
        }


        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _logger.Info("Executing " + JobId + " job");

                SystemInformationStatistic stat = new SystemInformationStatistic(_jobServerManager, _statisticService);

                stat.UpdateModel(cancellationToken);
            
        }

        public override string JobId => typeof(UpdateSystemInformationJob).Name;

        public override string Cron => "*/20 * * * *"; //Every 20 minutes
    }
}