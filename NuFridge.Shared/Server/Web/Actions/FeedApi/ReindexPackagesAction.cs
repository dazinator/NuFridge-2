using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Scheduler.Jobs;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class ReindexPackagesAction : IAction
    {
        private readonly ReindexPackagesForFeedJob _reIndexJob;

        public ReindexPackagesAction(ReindexPackagesForFeedJob reIndexJob)
        {
            _reIndexJob = reIndexJob;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int feedId = parameters.id;

            BackgroundJob.Enqueue(() => _reIndexJob.Execute(JobCancellationToken.Null, feedId));

            return new Response {StatusCode = HttpStatusCode.OK};
        }
    }
}