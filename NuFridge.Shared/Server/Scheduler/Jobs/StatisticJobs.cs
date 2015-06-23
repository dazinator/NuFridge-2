using FluentScheduler;
using NuFridge.Shared.Server.Scheduler.Jobs.Tasks;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class StatisticJobs : IJob
    {
        public void AddSchedule(Registry registry)
        {
            registry.Schedule<UpdateFeedPackageCountStatisticTask>().ToRunNow().AndEvery(15).Minutes();
            registry.Schedule<UpdateFeedDownloadCountStatisticTask>().ToRunNow().AndEvery(15).Minutes();
            registry.Schedule<UpdateSystemInformationTask>().ToRunNow().AndEvery(10).Minutes();
        }
    }
}