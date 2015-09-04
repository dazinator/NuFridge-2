using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Nancy;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Scheduler.Jobs.Definitions;
using NuFridge.Shared.Server.Security;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class ReindexPackagesAction : IAction
    {
        private readonly ReindexPackagesForFeedJob _reIndexJob;
        private readonly ILog _log = LogProvider.For<ReindexPackagesAction>();

        public ReindexPackagesAction(ReindexPackagesForFeedJob reIndexJob)
        {
            _reIndexJob = reIndexJob;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanReindexPackages });

            int feedId = parameters.id;

            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var processingJobs = monitoringApi.ProcessingJobs(0, int.MaxValue);

            foreach (var processingJob in processingJobs)
            {
                if (processingJob.Value.Job.Type == typeof (ReindexPackagesForFeedJob))
                {
                    //TODO Make a unit test for this
                    if (processingJob.Value.Job.Arguments.Count() != 2)
                    {
                        _log.Error("The reindex packages job has an unexpected number of arguments.");
                        throw new InvalidOperationException("The reindex packages job has an unexpected number of arguments.");
                    }
                    var strFeedIdForJob = processingJob.Value.Job.Arguments[1];
                    int feedIdForJob;
                    if (!int.TryParse(strFeedIdForJob, out feedIdForJob))
                    {
                        _log.Error("The reindex packages action could not read the feed id.");
                        throw new FormatException("The reindex packages action could not read the feed id.");
                    }

                    if (feedIdForJob == feedId)
                    {
                        return new TextResponse(HttpStatusCode.Conflict, "The requested feed is already reindexing its packages.");
                    }
                }
            }

            var enqueuedJobs = monitoringApi.EnqueuedJobs("default", 0, int.MaxValue);

            foreach (var enqueuedJob in enqueuedJobs)
            {
                if (enqueuedJob.Value.Job.Type == typeof(ReindexPackagesForFeedJob))
                {
                    //TODO Make a unit test for this
                    if (enqueuedJob.Value.Job.Arguments.Count() != 2)
                    {
                        _log.Error("The reindex packages job has an unexpected number of arguments.");
                        throw new InvalidOperationException("The reindex packages job has an unexpected number of arguments.");
                    }
                    var strFeedIdForJob = enqueuedJob.Value.Job.Arguments[1];
                    int feedIdForJob;
                    if (!int.TryParse(strFeedIdForJob, out feedIdForJob))
                    {
                        _log.Error("The reindex packages action could not read the feed id.");
                        throw new FormatException("The reindex packages action could not read the feed id.");
                    }

                    if (feedIdForJob == feedId)
                    {
                        return new TextResponse(HttpStatusCode.Conflict, "The requested feed is already pending a package reindex.");
                    }
                }
            }

            BackgroundJob.Enqueue(() => _reIndexJob.Execute(JobCancellationToken.Null, feedId));

            return new TextResponse(HttpStatusCode.OK, "The requested feed will be reindexed.");
        }
    }
}