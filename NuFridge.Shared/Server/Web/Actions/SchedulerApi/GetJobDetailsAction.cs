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
    class GetJobDetailsAction : IAction
    {
        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            string jobId = parameters.jobId;
            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();
            var jobDetailsDto = monitoringApi.JobDetails(jobId.ToString());
            return jobDetailsDto;
        }
    }
}