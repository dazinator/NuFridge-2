using System;
using System.Linq;
using Hangfire;
using Hangfire.Storage;
using Nancy;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web.Actions.SchedulerApi
{
    class GetSucceededJobsAction : IAction
    {
        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int page = int.Parse(module.Request.Query["page"]);
            int pageSize = int.Parse(module.Request.Query["pageSize"]);

            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

            var jobsCount = monitoringApi.SucceededListCount();
            var jobs = monitoringApi.SucceededJobs(pageSize * page, pageSize).OrderByDescending(jb => jb.Value.SucceededAt);

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
