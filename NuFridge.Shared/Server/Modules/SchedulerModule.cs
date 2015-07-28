using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using NuFridge.Shared.Server.Scheduler;
using NuFridge.Shared.Server.Scheduler.Jobs;
using NuFridge.Shared.Server.Web.Actions.SchedulerApi;
using Module = Autofac.Module;

namespace NuFridge.Shared.Server.Modules
{
    public class SchedulerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<JobBase>()
                .As<JobBase>()
                .AsSelf();

            builder.RegisterType<ReindexPackagesForFeedJob>().AsSelf();
            builder.RegisterType<ImportPackagesForFeedJob>().AsSelf();

            builder.RegisterType<JobServer>().As<IJobServer>().SingleInstance();
        }
    }
}