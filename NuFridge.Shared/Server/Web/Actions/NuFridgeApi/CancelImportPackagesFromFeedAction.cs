using System.Collections.Generic;
using Hangfire;
using Nancy;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class CancelImportPackagesFromFeedAction : IAction
    {

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanUploadPackages });

            string jobid = parameters.jobid;

            BackgroundJob.Delete(jobid);

            return new TextResponse(HttpStatusCode.OK);
        }
    }
}