using System.Reflection;
using Autofac;
using NuFridge.Shared.Scheduler;
using NuFridge.Shared.Scheduler.Jobs.Definitions;
using NuFridge.Shared.Scheduler.Servers;
using Module = Autofac.Module;

namespace NuFridge.Shared.Autofac
{
    public class SchedulerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            LoadManagers(builder);
            LoadJobs(builder);
            LoadInstances(builder);
        }

        protected virtual void LoadInstances(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<JobServerInstance>()
                .As<JobServerInstance>()
                .AsSelf().SingleInstance();
        }

        protected virtual void LoadManagers(ContainerBuilder builder)
        {
            builder.RegisterType<JobServerManager>().As<IJobServerManager>().SingleInstance();
        }

        protected virtual void LoadJobs(ContainerBuilder builder)
        {
            builder.RegisterType<ReindexPackagesForFeedJob>().AsSelf();
            builder.RegisterType<ImportPackagesForFeedJob>().AsSelf();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<JobBase>()
                .As<JobBase>()
                .AsSelf();
        }
    }
}