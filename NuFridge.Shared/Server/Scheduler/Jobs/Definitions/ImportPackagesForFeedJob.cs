using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hangfire;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet.Import;
using NuFridge.Shared.Server.Web.SignalR;
using NuGet;

namespace NuFridge.Shared.Server.Scheduler.Jobs.Definitions
{
    [Queue("filesystem")]
    public class ImportPackagesForFeedJob : JobExecution<PackageImportJob>
    {
        private readonly IPackageImporter _packageImporter;
        private readonly IPackageImportRepository _importRepository;
        private readonly IPackageImportJobItemService _jobItemService;
        private readonly ILog _log = LogProvider.For<ImportPackagesForFeedJob>();


        private PackageImportOptions Options { get; set; }

        public ImportPackagesForFeedJob(IPackageImporter packageImporter, IPackageImportRepository importRepository, IJobService jobService, IPackageImportJobItemService jobItemService) : base(jobService)
        {
            _packageImporter = packageImporter;
            _importRepository = importRepository;
            _jobItemService = jobItemService;
        }

        public void Start(IJobCancellationToken cancellationToken, int feedId, int userId, PackageImportOptions options)
        {
            WithFeedId(feedId);
            WithUserId(userId);
            WithCancellationToken(cancellationToken);

            WithTaskName("Package Import");
            Options = options;

            base.Start();
        }

        protected override void Execute()
        {
            int jobId = int.Parse(JobContext.JobId);

            _log.Info("Importing packages for feed id " + FeedId.Value);

            List<PackageImportJobItem> packageImportItems = _jobItemService.FindForJob(jobId).ToList();

            var remoteRepository = PackageRepositoryFactory.Default.CreateRepository(Options.FeedUrl);
            List<IPackage> packages = _importRepository.GetPackages(remoteRepository, Options);

            if (packages.Any())
            {
                foreach (var package in packages)
                {
                    if (!packageImportItems.Any(
                            pi => pi.PackageId == package.Id && pi.Version == package.Version.ToString()))
                    {
                        var item = _jobItemService.Insert(package, jobId);
                        packageImportItems.Add(item);
                    }
                }
            }

            List<PackageImportJobItem> toProcess = packageImportItems.Where(pi => !pi.StartedAt.HasValue).ToList();

            var hub = GlobalHost.ConnectionManager.GetHubContext<ImportPackagesHub>();

            UpdateCounters(hub, jobId, toProcess.Count, packageImportItems.Count - toProcess.Count);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int i = 0;

            foreach (var packageImportJobItem in toProcess)
            {
                if (stopwatch.Elapsed.TotalSeconds >= 5)
                {
                    stopwatch.Restart();

                    if (i > 0)
                    {
                        UpdateCounters(hub, jobId, Job.Scheduled - i, Job.Processed + i);
                        i = 0;
                    }
                }

                try
                {
                    packageImportJobItem.StartedAt = DateTime.UtcNow;

                    _packageImporter.ImportPackage(FeedId.Value, Options, packageImportJobItem);

                    packageImportJobItem.Success = true;
                    packageImportJobItem.CompletedAt = DateTime.UtcNow;
                    _jobItemService.Update(packageImportJobItem);
                }
                catch (PackageNotFoundException ex)
                {
                    packageImportJobItem.Log(LogLevel.Error, ex.Message);
                    packageImportJobItem.Success = false;
                    packageImportJobItem.CompletedAt = DateTime.UtcNow;
                    _jobItemService.Update(packageImportJobItem);
                }
                catch (InvalidPackageMetadataException ex)
                {
                    packageImportJobItem.Log(LogLevel.Error, ex.Message);
                    packageImportJobItem.Success = false;
                    packageImportJobItem.CompletedAt = DateTime.UtcNow;
                    _jobItemService.Update(packageImportJobItem);
                }
                catch (PackageConflictException ex)
                {
                    packageImportJobItem.Log(LogLevel.Error, ex.Message);
                    packageImportJobItem.Success = false;
                    packageImportJobItem.CompletedAt = DateTime.UtcNow;
                    _jobItemService.Update(packageImportJobItem);
                }
                catch (Exception ex)
                {
                    packageImportJobItem.Log(LogLevel.Error, ex.Message);
                    packageImportJobItem.Success = false;
                    packageImportJobItem.CompletedAt = DateTime.UtcNow;
                    _jobItemService.Update(packageImportJobItem);
                }

                i++;

                hub.Clients.Group(ImportPackagesHub.GetGroup(jobId)).packageProcessed(packageImportJobItem);
            }

            stopwatch.Stop();

            UpdateCounters(hub, jobId, Job.Scheduled - i, Job.Processed + i);
        }

        private void UpdateCounters(IHubContext hub, int jobId, int scheduled, int processed)
        {
            Job.Scheduled = scheduled;
            Job.Processed = processed;
            SaveJob();

            _log.Debug($"{Job.Scheduled} packages left to import. {Job.Processed} have been processed. Feed Id {FeedId}");

            hub.Clients.Group(ImportPackagesHub.GetGroup(jobId)).jobUpdated(Job);
        }
    }
}