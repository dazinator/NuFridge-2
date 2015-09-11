using Hangfire;

namespace NuFridge.Shared.Scheduler.Jobs.Definitions
{
    public abstract class JobBase
    {
        public abstract void Execute(IJobCancellationToken cancellationToken);
        public abstract string JobId { get; }
        public virtual string Cron { get; }
        public virtual bool TriggerOnRegister => true;
    }
}