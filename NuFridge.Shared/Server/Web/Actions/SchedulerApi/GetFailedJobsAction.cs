using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Nancy;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web.Actions.SchedulerApi
{
    class GetFailedJobsAction : IAction
    {
        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int page = int.Parse(module.Request.Query["page"]);
            int pageSize = int.Parse(module.Request.Query["pageSize"]);

            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

            var jobsCount = monitoringApi.FailedCount();
            var jobs = monitoringApi.FailedJobs(pageSize * page, pageSize);

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
