using System.Threading;
using Hangfire;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Server.Web.SignalR;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    public class ImportPackagesForFeedJob
    {
        public void Execute(IJobCancellationToken cancellationToken, int feedId)
        {
            var importStatus = new FeedImportStatus(feedId);

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ImportPackagesHub>();

            hubContext.Clients.Group(ImportPackagesHub.GetGroup(feedId)).importPackagesUpdate(importStatus);

            Thread.Sleep(5000);

            importStatus.TotalCount = 100;
            importStatus.CompletedCount = 10;
            importStatus.RemainingCount = 90;
            hubContext.Clients.Group(ImportPackagesHub.GetGroup(feedId)).importPackagesUpdate(importStatus);

            Thread.Sleep(5000);

            importStatus.TotalCount = 100;
            importStatus.CompletedCount = 100;
            importStatus.RemainingCount = 0;
            hubContext.Clients.Group(ImportPackagesHub.GetGroup(feedId)).importPackagesUpdate(importStatus);
        }

        public class FeedImportStatus
        {
            public FeedImportStatus(int feedId)
            {
                FeedId = feedId;
                RemainingCount = 0;
                CompletedCount = 0;
                FailedCount = 0;
                TotalCount = 0;
            }

            public int FeedId { get; set; }

            public int RemainingCount { get; set; }
            public int CompletedCount { get; set; }
            public int FailedCount { get; set; }
            public int TotalCount { get; set; }
        }
    }
}