using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Extensions;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class GetBackgroundJobsAction : IAction
    {
        private readonly IJobService _jobService;

        public GetBackgroundJobsAction(IJobService jobService)
        {
            _jobService = jobService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanViewFeeds });

            int pageNumber = module.Request.Query["page"];
            int size = module.Request.Query["size"];


            int? feedId = parameters.id.HasValue ? parameters.id : null;

            int totalResults;

            var jobs = feedId.HasValue
                ? _jobService.FindForFeed(feedId.Value, pageNumber, size, out totalResults)
                : _jobService.Find(pageNumber, size, out totalResults);

            return module.Negotiate.WithModel(new
            {
                Jobs = jobs,
                Total = totalResults
            }).WithStatusCode(HttpStatusCode.OK);
        }
    }
}