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
    class GetScheduledJobsAction : IAction
    {
        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            using (IStorageConnection connection = JobStorage.Current.GetConnection())
            {
                List<RecurringJobDto> recurringJobs = connection.GetRecurringJobs();

                return recurringJobs;
            }
        }
    }
}
