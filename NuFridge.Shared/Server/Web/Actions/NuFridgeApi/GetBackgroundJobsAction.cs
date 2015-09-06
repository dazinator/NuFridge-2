using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
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

            int feedId = parameters.id;

            var pageNumber = module.Request.Query["page"];
            var size = module.Request.Query["size"];

            IEnumerable<Job> jobs = _jobService.FindForFeed(feedId, pageNumber, size);

            return module.Negotiate.WithModel(jobs).WithStatusCode(HttpStatusCode.OK);
        }
    }
}