﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hangfire;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.NuGet.Repository;
using NuFridge.Shared.Web.SignalR;

namespace NuFridge.Shared.Scheduler.Jobs.Definitions
{
    [Queue("filesystem")]
    public class ImportPackagesForFeedJob : JobExecution<PackageImportJob>
    {
        private readonly IRemotePackageImporter _remotePackageImporter;
        private readonly IRemotePackageRepository _repository;
        private readonly IPackageImportJobItemService _jobItemService;
        private readonly ILog _log = LogProvider.For<ImportPackagesForFeedJob>();


        private RemotePackageImportOptions Options { get; set; }

        public ImportPackagesForFeedJob(IRemotePackageImporter remotePackageImporter, IRemotePackageRepository repository, IJobService jobService, IPackageImportJobItemService jobItemService) : base(jobService)
        {
            _remotePackageImporter = remotePackageImporter;
            _repository = repository;
            _jobItemService = jobItemService;
        }

        public void Start(IJobCancellationToken cancellationToken, int feedId, int userId, RemotePackageImportOptions options)
        {
            WithFeedId(feedId);
            WithUserId(userId);
            WithCancellationToken(cancellationToken);

            WithTaskName("Package import from " + options.FeedUrl);
            Options = options;

            Start();
        }

        protected override void Execute()
        {
            int jobId = int.Parse(JobContext.JobId);

            _log.Info("Importing packages for feed id " + FeedId.Value);

            var hub = GlobalHost.ConnectionManager.GetHubContext<ImportPackagesHub>();

            IEnumerable<RemoteRemotePackageRepository.PackageImportResult> results = _repository.GetPackages(Options);

            List<PackageImportJobItem> packageImportItems = _jobItemService.FindForJob(jobId).ToList();

            Stopwatch stopwatch = new Stopwatch();

            foreach (var result in results)
            {
                try
                {
                    CancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception)
                {
                    _log.Warn($"The package import for feed id {FeedId} has been cancelled.");
                    hub.Clients.Group(ImportPackagesHub.GetGroup(jobId)).importCancelled();
                    CancelJob();
                    throw;
                }

                Job.Scheduled = result.TotalCount;

                if (stopwatch.Elapsed.TotalSeconds >= 5 || !stopwatch.IsRunning)
                {
                    stopwatch.Restart();

                    UpdateCounters(hub, jobId);
                }

                PackageImportJobItem item = packageImportItems.SingleOrDefault(pi => pi.PackageId == result.Package.Id && pi.Version == result.Package.Version);
                if (item == null)
                {
                    item = _jobItemService.Insert(result.Package, jobId);
                    packageImportItems.Add(item);
                }

                if (!item.CompletedAt.HasValue)
                {
                    try
                    {
                        _remotePackageImporter.ImportPackage(FeedId.Value, Options, result.Package, item);

                        item.Success = true;
                        item.CompletedAt = DateTime.UtcNow;
                        _jobItemService.Update(item);
                    }
                    catch (PackageNotFoundException ex)
                    {
                        item.Log(LogLevel.Error, ex.Message);
                        item.Success = false;
                        item.CompletedAt = DateTime.UtcNow;
                        _jobItemService.Update(item);
                    }
                    catch (InvalidPackageMetadataException ex)
                    {
                        item.Log(LogLevel.Error, ex.Message);
                        item.Success = false;
                        item.CompletedAt = DateTime.UtcNow;
                        _jobItemService.Update(item);
                    }
                    catch (PackageConflictException ex)
                    {
                        item.Log(LogLevel.Error, ex.Message);
                        item.Success = false;
                        item.CompletedAt = DateTime.UtcNow;
                        _jobItemService.Update(item);
                    }
                    catch (Exception ex)
                    {
                        item.Log(LogLevel.Error, ex.Message);
                        item.Success = false;
                        item.CompletedAt = DateTime.UtcNow;
                        _jobItemService.Update(item);
                    }

                    Job.Processed++;
                    hub.Clients.Group(ImportPackagesHub.GetGroup(jobId)).packageProcessed(item);
                }
            }

            stopwatch.Stop();

            UpdateCounters(hub, jobId);

            _log.Info($"Finished package import for feed id {FeedId}");
        }

        private void UpdateCounters(IHubContext hub, int jobId)
        {
            SaveJob();

            _log.Debug($"{Job.Scheduled - Job.Processed} packages left to import. {Job.Processed} have been processed. Feed Id {FeedId}");

            hub.Clients.Group(ImportPackagesHub.GetGroup(jobId)).loadDetailedJob(Job);
        }
    }
}