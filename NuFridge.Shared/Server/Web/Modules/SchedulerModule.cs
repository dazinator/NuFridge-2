using Autofac;
using Nancy;
using NuFridge.Shared.Server.Web.Actions.SchedulerApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class SchedulerModule : NancyModule
    {
        public SchedulerModule(IContainer container)
        {
            Get["api/scheduler/jobs/enqueued"] = p => container.Resolve<GetEnqueuedJobsAction>().Execute(p, this);

            Get["api/scheduler/jobs/processing"] = p => container.Resolve<GetProcessingJobsAction>().Execute(p, this);

            Get["api/scheduler/jobs/succeeded"] = p => container.Resolve<GetSucceededJobsAction>().Execute(p, this);
        
            Get["api/scheduler/jobs/failed"] = p => container.Resolve<GetFailedJobsAction>().Execute(p, this);

            Get["api/scheduler/jobs/scheduled"] = p => container.Resolve<GetScheduledJobsAction>().Execute(p, this);

            Get["api/scheduler/jobs/deleted"] = p => container.Resolve<GetDeletedJobsAction>().Execute(p, this);

            Get["api/scheduler/jobs/details/{jobId}"] = p => container.Resolve<GetJobDetailsAction>().Execute(p, this);

            Get["api/scheduler/servers"] = p => container.Resolve<GetServersAction>().Execute(p, this);
        }
    }
}