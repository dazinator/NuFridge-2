using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class GetPackageAuditHistoryAction : IAction
    {
        private readonly IPackageDownloadService _packageDownloadService;
        private readonly IPackageService _packageService;

        public GetPackageAuditHistoryAction(IPackageDownloadService packageDownloadService, IPackageService packageService)
        {
            _packageDownloadService = packageDownloadService;
            _packageService = packageService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int feedId = int.Parse(parameters.id);

            return new
            {
                Downloads = _packageDownloadService.GetLatestDownloads(feedId),
                Uploads = _packageService.GetLatestUploads(feedId)
            };
        }
    }
}