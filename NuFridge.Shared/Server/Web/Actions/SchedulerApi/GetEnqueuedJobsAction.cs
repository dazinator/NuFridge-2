using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Nancy;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web.Actions.SchedulerApi
{
    class GetEnqueuedJobsAction : IAction
    {
        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int page = int.Parse(module.Request.Query["page"]);
            int pageSize = int.Parse(module.Request.Query["pageSize"]);

            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

            var jobsCount = monitoringApi.EnqueuedCount("filesystem") + monitoringApi.EnqueuedCount("background");
            List<EnqueuedJobDto> jobs = monitoringApi.EnqueuedJobs("filesystem", pageSize * page, pageSize).Select(s => s.Value).Union(monitoringApi.EnqueuedJobs("background", pageSize * page, pageSize).Select(s => s.Value)).OrderByDescending(jb => jb.EnqueuedAt).ToList();

            var totalPages = (int)Math.Ceiling((double)jobsCount / pageSize);

            return new
            {
                TotalCount = jobsCount,
                TotalPages = totalPages,
                Results = jobs
            };
        }
    }
}
