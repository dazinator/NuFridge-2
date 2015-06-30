using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;
using NuFridge.Shared.Server.Scheduler.Jobs.Tasks;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class PackageJobs : IJob
    {
        public void AddSchedule(Registry registry)
        {
            registry.Schedule<RunPackageRetentionPoliciesTask>().ToRunEvery(1).Days().At(0, 0);
        }
    }
}