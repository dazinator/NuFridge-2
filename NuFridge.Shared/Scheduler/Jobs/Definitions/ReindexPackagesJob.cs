using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.NuGet.Repository;

namespace NuFridge.Shared.Scheduler.Jobs.Definitions
{
    [Queue("filesystem")]
    public class ReindexPackagesJob : JobBase
    {
        private readonly IFeedService _feedService;
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;
        private readonly IFeedConfigurationService _feedConfigurationService;
        private readonly IPackageService _packageService;
        private readonly ILog _log = LogProvider.For<ReindexPackagesJob>();

        public override string JobId => typeof(ReindexPackagesJob).Name;
        public override string Cron => Hangfire.Cron.Weekly(DayOfWeek.Monday, 4, 0); //Every monday at 04:00

        public ReindexPackagesJob(IFeedService feedService, IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IFeedConfigurationService feedConfigurationService, IPackageService packageService)
        {
            _feedService = feedService;
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
            _feedConfigurationService = feedConfigurationService;
            _packageService = packageService;
        }

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(10)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _log.Info("Executing " + JobId + " job");

            IEnumerable<Feed> feeds = _feedService.GetAll().ToList();

            string jobId = null;
            string previousFeedName = null;
            foreach (var feed in feeds)
            {
                ReindexPackagesForFeedJob job = new ReindexPackagesForFeedJob(_packageRepositoryFactory, _store, _feedConfigurationService, _packageService);

                if (jobId == null)
                {
                    _log.Info("Enqueuing reindex packages job for feed " + feed.Name);
                    jobId = BackgroundJob.Enqueue(() => job.Execute(JobCancellationToken.Null, feed.Id));
                }
                else
                {
                    _log.Info("Enqueuing reindex packages job for feed " + feed.Name + " after the " + previousFeedName + " feed completes reindexing");
                    jobId = BackgroundJob.ContinueWith(jobId, () => job.Execute(JobCancellationToken.Null, feed.Id));
                }

                previousFeedName = feed.Name;
            }
        }
    }
}