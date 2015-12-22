using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NuFridge.Shared.Autofac;
using NuFridge.Shared.Scheduler.Jobs.Definitions;
using NuFridge.Tests.Scheduler.Jobs.Definitions;

namespace NuFridge.Tests.Autofac
{
    public class TestableSchedulerModule : SchedulerModule
    {
        protected override void LoadJobs(ContainerBuilder builder)
        {
            builder.RegisterType<ReindexPackagesForFeedJob>().AsSelf();
            builder.RegisterType<ImportPackagesForFeedJob>().AsSelf();
            builder.RegisterType<TestRunPackageRetentionPoliciesJob>().AsSelf();
        }
    }
}
