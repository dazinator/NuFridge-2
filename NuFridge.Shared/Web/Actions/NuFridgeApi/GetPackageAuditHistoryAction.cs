﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class GetPackageAuditHistoryAction : IAction
    {
        private readonly IPackageDownloadService _packageDownloadService;
        private readonly IPackageService _packageService;
        private readonly IJobService _jobService;

        public GetPackageAuditHistoryAction(IPackageDownloadService packageDownloadService, IPackageService packageService, IJobService jobService)
        {
            _packageDownloadService = packageDownloadService;
            _packageService = packageService;
            _jobService = jobService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanViewFeedHistory });

            int feedId = int.Parse(parameters.id);

            List<AuditItem> results = new List<AuditItem>();

            IEnumerable<PackageDownload> downloads = _packageDownloadService.GetLatestDownloads(feedId);
            IEnumerable<PackageUpload> uploads = _packageService.GetLatestUploads(feedId);
            IEnumerable<Job> jobs = _jobService.FindForFeed(feedId);


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

            results.AddRange(jobs.Select(import => new AuditItem
            {
                Title = import.Name,
                Date = import.CreatedAt,
                Type = 2
            }));

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