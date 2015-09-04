using Hangfire;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet.Import;

namespace NuFridge.Shared.Server.Scheduler.Jobs.Definitions
{
    [Queue("filesystem")]
    public class CleanupExpiredImportPackagesJob : JobBase
    {
        public override string JobId => typeof(CleanupExpiredImportPackagesJob).Name;
        public override bool TriggerOnRegister => false;
        public override string Cron => Hangfire.Cron.Daily(20);

        private readonly ILog _log = LogProvider.For<CleanupTempFilesJob>();

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(10)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _log.Info("Executing " + JobId + " job");

            PackageImportProgressTracker.Instance.RemoveExpiredJobs();
        }
    }
}