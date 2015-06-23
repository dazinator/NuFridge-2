using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentScheduler;
using NuFridge.Shared.Server.Scheduler;
using NuFridge.Shared.Server.Scheduler.Jobs;
using Module = Autofac.Module;

namespace NuFridge.Shared.Server.Modules
{
    public class SchedulerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AssignableTo<IJob>().As<IJob>();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AssignableTo<ITask>().AsSelf();
        }
    }
}