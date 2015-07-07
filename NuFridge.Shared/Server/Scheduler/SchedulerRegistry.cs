using System.Collections.Generic;
using FluentScheduler;
using NuFridge.Shared.Server.Scheduler.Jobs;

namespace NuFridge.Shared.Server.Scheduler
{
    public class SchedulerRegistry : Registry
    {
        public SchedulerRegistry(IEnumerable<IJob> registries)
        {
            foreach (var registry in registries)
            {
                registry.AddSchedule(this);
            }
        }
    }
}
