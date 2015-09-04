using System.Collections.Generic;
using Hangfire;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.NuGet.Import;
using NuFridge.Shared.Server.Scheduler.Jobs.Definitions;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    class ImportPackagesFromFeedAction : IAction 
    {
        private readonly IUserService _userService;
        private readonly ImportPackagesForFeedJob _importPackagesForFeedJob;

        public ImportPackagesFromFeedAction(ImportPackagesForFeedJob importJob, IUserService userService)
        {
            _userService = userService;
            _importPackagesForFeedJob = importJob;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanUploadPackages });

            int feedId = parameters.id;

            PackageImportOptions options = module.Bind<PackageImportOptions>();

            var userId = _userService.GetLoggedInUserId(module);

            string errorMessage;
            if (!options.IsValid(out errorMessage))
            {
                return new TextResponse(HttpStatusCode.BadRequest, errorMessage);
            }

            string jobId = BackgroundJob.Enqueue(() => _importPackagesForFeedJob.Start(JobCancellationToken.Null, feedId, userId, options));

            return new TextResponse(HttpStatusCode.OK, jobId);
        }
    }
}