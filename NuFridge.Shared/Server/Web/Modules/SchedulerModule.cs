using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Owin;
using Nancy;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class SchedulerModule : NancyModule
    {
        public SchedulerModule()
        {
            Get["api/scheduler/stats"] = p =>
            {
                this.RequiresAuthentication();

                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

                return new
                {
                    Failed = monitoringApi.FailedCount(),
                    Scheduled = monitoringApi.ScheduledCount(),
                    Succeeded = monitoringApi.SucceededListCount(),
                    Processing = monitoringApi.ProcessingCount()
                };
            };

            Get["api/scheduler/jobs/enqueued"] = p =>
            {
                this.RequiresAuthentication();

                var queues = JobStorage.Current.GetMonitoringApi().Queues();


                return Negotiate.WithModel(queues);
            };

            Get["api/scheduler/jobs/processing"] = p =>
            {
                var module = this;

                this.RequiresAuthentication();

                int page = int.Parse(module.Request.Query["page"]);
                int pageSize = int.Parse(module.Request.Query["pageSize"]);

                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

                var jobsCount = monitoringApi.ProcessingCount();
                var jobs = monitoringApi.ProcessingJobs(pageSize * page, pageSize);

                var totalPages = (int)Math.Ceiling((double)jobsCount / pageSize);

                return new
                {
                    TotalCount = jobsCount,
                    TotalPages = totalPages,
                    Results = jobs
                };
            };

            Get["api/scheduler/jobs/succeeded"] = p =>
            {
                var module = this;

                this.RequiresAuthentication();

                int page = int.Parse(module.Request.Query["page"]);
                int pageSize = int.Parse(module.Request.Query["pageSize"]);

                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

                var jobsCount = monitoringApi.SucceededListCount();
                var jobs = monitoringApi.SucceededJobs(pageSize * page, pageSize);

                var totalPages = (int)Math.Ceiling((double)jobsCount / pageSize);

                return new
                {
                    TotalCount = jobsCount,
                    TotalPages = totalPages,
                    Results = jobs
                };
            };

            Get["api/scheduler/jobs/failed"] = p =>
            {
                var module = this;

                this.RequiresAuthentication();

                int page = int.Parse(module.Request.Query["page"]);
                int pageSize = int.Parse(module.Request.Query["pageSize"]);

                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

                var jobsCount = monitoringApi.FailedCount();
                var jobs = monitoringApi.FailedJobs(pageSize * page, pageSize);

                var totalPages = (int)Math.Ceiling((double)jobsCount / pageSize);

                return new
                {
                    TotalCount = jobsCount,
                    TotalPages = totalPages,
                    Results = jobs
                };
            };

            Get["api/scheduler/jobs/awaiting"] = p =>
            {
                var module = this;

                this.RequiresAuthentication();

                int page = int.Parse(module.Request.Query["page"]);
                int pageSize = int.Parse(module.Request.Query["pageSize"]);

                using (IStorageConnection connection = JobStorage.Current.GetConnection())
                {
                    JobStorageConnection storageConnection = connection as JobStorageConnection;

                    int total = (int)storageConnection.GetSetCount("awaiting");
                    var results = storageConnection.GetRangeFromSet("awaiting", pageSize * page, (pageSize * page) + pageSize - 1);

                    var totalPages = (int)Math.Ceiling((double)total / pageSize);

                    return new
                    {
                        TotalCount = total,
                        TotalPages = totalPages,
                        Results = results
                    };
                }
            };

            Get["api/scheduler/jobs/scheduled"] = p =>
            {
                var module = this;

                module.RequiresAuthentication();

                using (IStorageConnection connection = JobStorage.Current.GetConnection())
                {
                    List<RecurringJobDto> recurringJobs = connection.GetRecurringJobs();

                    return recurringJobs;
                }
            };

            Get["api/scheduler/jobs/deleted"] = p =>
            {
                var module = this;

                this.RequiresAuthentication();

                int page = int.Parse(module.Request.Query["page"]);
                int pageSize = int.Parse(module.Request.Query["pageSize"]);

                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

                var jobsCount = monitoringApi.DeletedListCount();
                JobList<DeletedJobDto> jobs = monitoringApi.DeletedJobs(pageSize * page, pageSize);

                var totalPages = (int)Math.Ceiling((double)jobsCount / pageSize);

                return new
                {
                    TotalCount = jobsCount,
                    TotalPages = totalPages,
                    Results = jobs
                };
            };

            Get["api/scheduler/jobs/details/{jobId}"] = p =>
            {
                this.RequiresAuthentication();

                string jobId = p.jobId;
                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();
                var jobDetailsDto = monitoringApi.JobDetails(jobId.ToString());
                return jobDetailsDto;
            };

            Get["api/scheduler/servers"] = p =>
            {
                this.RequiresAuthentication();

                IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

                var servers = monitoringApi.Servers();

                return new
                {
                    Results = servers
                };
            };

            Get["api/scheduler/retries"] = p =>
            {
                var module = this;

                this.RequiresAuthentication();

                int page = int.Parse(module.Request.Query["page"]);
                int pageSize = int.Parse(module.Request.Query["pageSize"]);

                using (IStorageConnection connection = JobStorage.Current.GetConnection())
                {
                    JobStorageConnection storageConnection = connection as JobStorageConnection;

                    int total = (int)storageConnection.GetSetCount("retries");
                    var results = storageConnection.GetRangeFromSet("retries", pageSize * page, (pageSize * page) + pageSize - 1);

                    var totalPages = (int)Math.Ceiling((double)total / pageSize);

                    return new
                    {
                        TotalCount = total,
                        TotalPages = totalPages,
                        Results = results
                    };
                }
            };
        }
    }
}