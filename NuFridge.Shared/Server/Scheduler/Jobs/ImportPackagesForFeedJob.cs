using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet.Import;
using NuFridge.Shared.Server.Web.SignalR;
using NuGet;

namespace NuFridge.Shared.Server.Scheduler.Jobs
{
    [Queue("filesystem")]
    public class ImportPackagesForFeedJob
    {
        private readonly IFeedService _feedService;
        private readonly IPackageImporter _packageImporter;
        private readonly IPackageImportRepository _importRepository;
        private readonly ILog _log = LogProvider.For<ImportPackagesForFeedJob>();

        public ImportPackagesForFeedJob(IFeedService feedService, IPackageImporter packageImporter, IPackageImportRepository importRepository)
        {
            _feedService = feedService;
            _packageImporter = packageImporter;
            _importRepository = importRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Execute(IJobCancellationToken cancellationToken, int feedId, PackageImportOptions options)
        {
            _log.Info("Running import packages job for feed id " + feedId);

            string jobId = JobContext.JobId;

            if (options.CheckLocalCache && _feedService.GetCount() <= 1)
            {
                _log.Debug("Disabled local cache for package import for feed id " + feedId);

                options.CheckLocalCache = false;
            }

            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ImportPackagesHub>();

            List<IPackage> packages;

            try
            {
                var remoteRepository = PackageRepositoryFactory.Default.CreateRepository(options.FeedUrl);
                packages = _importRepository.GetPackages(remoteRepository, options);
            }
            catch (Exception ex)
            {
                _log.ErrorException("There was an error getting the list of packages to import.", ex);
                PackageImportProgressTracker.Instance.ReportStartFailure(hubContext, jobId, "The package import failed to start. " + ex.Message);
                throw;
            }

            _log.Info("Found " + packages.Count + " for import for feed id " + feedId + " from " + options.FeedUrl);

            PackageImportProgressTracker.Instance.AddJob(hubContext, jobId, feedId, packages.Count());

            ProcessPackages(feedId, options.FeedUrl, packages, jobId, options.CheckLocalCache);
        }

        private void ProcessPackages(int feedId, string feedUrl, List<IPackage> packages, string jobId, bool useLocalPackages)
        {
            _log.Debug("Enqueuing packages for import for feed id " + feedId);

            Parallel.ForEach(packages, package =>
            {
                BackgroundJob.Enqueue(() => _packageImporter.ImportPackage(jobId, feedId, feedUrl, package.Id, package.Version.ToString(), useLocalPackages));
            });

            _log.Info($"{packages.Count()} packages have been enqueued for import on feed id {feedId}");

            PackageImportProgressTracker.Instance.WaitUntilComplete(JobContext.JobId);
        }
    }
}