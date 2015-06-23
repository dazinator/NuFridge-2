using FluentScheduler;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public interface IJob
    {
        void AddSchedule(Registry registry);
    }
}