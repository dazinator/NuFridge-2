using Hangfire;
using Hangfire.Logging;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class UpdateSystemInformationJob : JobBase
    {
        private readonly IJobServer _jobServer;
        private IStore Store { get; set; }
        private readonly ILog _logger = LogProvider.For<UpdateSystemInformationJob>();

        public UpdateSystemInformationJob(IStore store, IJobServer jobServer) 
        {
            _jobServer = jobServer;
            Store = store;
        }

        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _logger.Info("Executing " + JobId + " job");

            using (ITransaction transaction = Store.BeginTransaction())
            {
                SystemInformationStatistic stat = new SystemInformationStatistic(transaction, _jobServer);

                stat.UpdateModel(cancellationToken);
            }
        }

        public override string JobId => typeof(UpdateSystemInformationJob).Name;

        public override string Cron => "*/20 * * * *"; //Every 20 minutes
    }
}