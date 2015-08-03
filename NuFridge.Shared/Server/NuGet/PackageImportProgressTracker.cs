using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Web.SignalR;

namespace NuFridge.Shared.Server.NuGet
{
    public class PackageImportProgressTracker
    {
        public const int RetryCount = 2;
        private const int TotalExecutionCount = RetryCount + 1;
        private static readonly Lazy<PackageImportProgressTracker> _instance = new Lazy<PackageImportProgressTracker>(() => new PackageImportProgressTracker());
        private readonly ConcurrentDictionary<string, PackageImportProgress> _dictionary = new ConcurrentDictionary<string, PackageImportProgress>();
        private readonly ILog _log = LogProvider.For<PackageImportProgressTracker>();

        private PackageImportProgressTracker()
        {

        }

        public void IncrementSuccessCount(string jobId, PackageImportProgressAuditItem item)
        {
            item.Attempts++;

            _dictionary.AddOrUpdate(jobId, new PackageImportProgress(), delegate (string key, PackageImportProgress progress)
            {
                var auditItem = progress.Summary.FailedPackages.SingleOrDefault(fp => fp.PackageId == item.PackageId && fp.Version == item.Version);
                if (auditItem != null)
                {
                    progress.Summary.FailedPackages.Remove(auditItem);
                }

                progress.Summary.ImportedPackages.Add(item);
                progress.Counters.ImportedCount++;
                return progress;
            });
        }

        public void IncrementFailureCount(string jobId, PackageImportProgressAuditItem item)
        {
            _dictionary.AddOrUpdate(jobId, new PackageImportProgress(), delegate (string key, PackageImportProgress progress)
            {
                var auditItem = progress.Summary.FailedPackages.SingleOrDefault(fp => fp.PackageId == item.PackageId && fp.Version == item.Version);
                if (auditItem == null)
                {
                    progress.Summary.FailedPackages.Add(item);
                    item.Attempts++;
                }
                else
                {
                    auditItem.Attempts++;

                    if (!string.IsNullOrWhiteSpace(item.Error))
                    {
                        auditItem.Error = item.Error;
                    }

                    if (auditItem.Attempts >= TotalExecutionCount)
                    {
                        progress.Counters.FailedCount++;
                    }
                }

                return progress;
            });
        }

        private void SendFinishedUpdate(string jobId)
        {
            PackageImportProgress progress;

            _dictionary.TryRemove(jobId, out progress);

            SendInProgressUpdate(progress);

            _log.Info("Finished package import for feed id " + progress.FeedId + ". Sending final update to connected clients.");

            progress.HubContext.Clients.Group(ImportPackagesHub.GetGroup(progress.JobId)).updateImportStatus( new { progress.Summary } );
        }

        private void SendInProgressUpdate(PackageImportProgress progress)
        {
            _log.Info($"{progress.Counters.ImportedCount} of {progress.Counters.TotalCount} packages have been imported for feed id {progress.FeedId}");

            progress.HubContext.Clients.Group(ImportPackagesHub.GetGroup(progress.JobId)).updateImportStatus(  new { progress.Counters } );
        }

        public void WaitUntilComplete(string jobId)
        {
            PackageImportProgress progress;

            bool isImportingPackages = true;

            int failed = 0;
            int imported = 0;

            while (isImportingPackages)
            {
                if (_dictionary.TryGetValue(jobId, out progress))
                {
                    if (progress.Counters.FailedCount != failed || progress.Counters.ImportedCount != imported)
                    {
                        SendInProgressUpdate(progress);

                        failed = progress.Counters.FailedCount;
                        imported = progress.Counters.ImportedCount;
                    }

                    int combinedTotal = progress.Counters.FailedCount + progress.Counters.ImportedCount;
                    if (combinedTotal == progress.Counters.TotalCount)
                    {
                        isImportingPackages = false;
                    }
                }

                if (isImportingPackages)
                {
                    Thread.Sleep(5000);
                }
            }

            _log.Debug("Setting feed import to complete for job id " + jobId);

            if (_dictionary.TryGetValue(jobId, out progress))
            {
                progress.Summary.IsCompleted = true;

                SendFinishedUpdate(jobId);
            }
        }

        public void AddJob(IHubContext hubContext, string jobId, int feedId, int total)
        {
            _log.Info("Tracking package import job for feed id " + feedId);

            PackageImportProgress progress = new PackageImportProgress {FeedId = feedId, HubContext = hubContext, JobId = jobId};
            progress.Counters.TotalCount = total;

            _dictionary.TryAdd(jobId, progress);

            SendInProgressUpdate(progress);
        }

        public static PackageImportProgressTracker Instance
        {
            get
            {
                if (_instance.IsValueCreated)
                {
                    return _instance.Value;
                }

                return _instance.Value;
            }
        }

        public void ReportStartFailure(IHubContext hubContext, string jobId, string message)
        {
            hubContext.Clients.Group(ImportPackagesHub.GetGroup(jobId)).startFailure(message);
        }
    }

    public class PackageImportProgress
    {
        public IHubContext HubContext { get; set; }
        public int FeedId { get; set; }
        public string JobId { get; set; }

        public PackageImportProgressSummary Summary = new PackageImportProgressSummary();
        public PackageImportProgressCounters Counters = new PackageImportProgressCounters();
    }

    public class PackageImportProgressSummary
    {
        public bool IsCompleted { get; set; }

        public List<PackageImportProgressAuditItem> ImportedPackages = new List<PackageImportProgressAuditItem>();
        public List<PackageImportProgressAuditItem> FailedPackages = new List<PackageImportProgressAuditItem>();
    }

    public class PackageImportProgressCounters
    {
        public PackageImportProgressCounters()
        {
            ImportedCount = 0;
            FailedCount = 0;
        }

        public int TotalCount { get; set; }
        public int ImportedCount { get; set; }
        public int FailedCount { get; set; }
    }

    public class PackageImportProgressAuditItem
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string Error { get; set; }
        public int Attempts { get; set; }

        public PackageImportProgressAuditItem() : this(null, null)
        {
            
        }

        public PackageImportProgressAuditItem(string packageId, string version) : this(packageId, version, null)
        {
            
        }

        public PackageImportProgressAuditItem(string pacakgeId, string version, string error)
        {
            PackageId = pacakgeId;
            Version = version;
            Error = error;
        }
    }
}