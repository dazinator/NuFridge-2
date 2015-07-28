using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Nancy;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Scheduler.Jobs;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    class ImportPackagesFromFeedAction : IAction 
    {
        private readonly ImportPackagesForFeedJob _importPackagesForFeedJob;

        public ImportPackagesFromFeedAction(ImportPackagesForFeedJob importJob)
        {
            _importPackagesForFeedJob = importJob;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int feedId = parameters.id;

            BackgroundJob.Enqueue(() => _importPackagesForFeedJob.Execute(JobCancellationToken.Null, feedId));

            return new TextResponse(HttpStatusCode.OK, "The requested feed will be imported.");
        }
    }
}
