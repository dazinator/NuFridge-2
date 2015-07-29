using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Scheduler.Jobs;
using FeedImportOptions = NuFridge.Shared.Server.Scheduler.Jobs.ImportPackagesForFeedJob.FeedImportOptions;

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

            FeedImportOptions options = module.Bind<FeedImportOptions>();


            string errorMessage;
            if (!options.IsValid(out errorMessage))
            {
                return new TextResponse(HttpStatusCode.BadRequest, errorMessage);
            }

            BackgroundJob.Enqueue(() => _importPackagesForFeedJob.Execute(JobCancellationToken.Null, feedId, options));

            return new TextResponse(HttpStatusCode.OK, "The requested feed will be imported.");
        }
    }
}
