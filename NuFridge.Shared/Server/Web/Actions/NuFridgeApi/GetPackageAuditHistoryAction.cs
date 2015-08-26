using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
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

            List<AuditItem> results = new List<AuditItem>();

            IEnumerable<PackageDownload> downloads = _packageDownloadService.GetLatestDownloads(feedId);
            IEnumerable<PackageUpload> uploads = _packageService.GetLatestUploads(feedId);

            foreach (var packageDownload in downloads)
            {
                results.Add(new AuditItem
                {
                    Title = packageDownload.PackageId + " v" + packageDownload.Version + " downloaded",
                    Date = packageDownload.DownloadedAt,
                    Type = 0,
                    Description = "from " + packageDownload.IPAddress
                });
            }

            results.AddRange(uploads.Select(packageUpload => new AuditItem
            {
                Title = packageUpload.Id + " v" + packageUpload.Version + " uploaded",
                Date = packageUpload.Published,
                Type = 1
            }));

            results = results.OrderByDescending(r => r.Date).ToList();

            return new
            {
                Results = results
            };
        }

        public class AuditItem
        {
            public string Title { get; set; }
            public DateTime Date { get; set; }
            public int Type { get; set; }
            public string Description { get; set; }
        }
    }
}