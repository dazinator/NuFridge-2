using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Web.SignalR
{
    public class ImportPackagesHub : Hub
    {
        public const string GroupPrefix = "ImportPackagesHub:";

        public async Task Subscribe(int jobId)
        {
            await Groups.Add(Context.ConnectionId, GetGroup(jobId));

            var jobItemService = GlobalHost.DependencyResolver.Resolve<IPackageImportJobItemService>();
            var jobService = GlobalHost.DependencyResolver.Resolve<IJobService>();

            Clients.Caller.jobUpdated(jobService.Find<PackageImportJob>(jobId));
            Clients.Caller.loadPackages(jobItemService.FindForJob(jobId).ToList());
        }

        public async Task Unsubscribe(int jobId)
        {
            await Groups.Remove(Context.ConnectionId, GetGroup(jobId));
        }

        public static string GetGroup(int jobId)
        {
            return GroupPrefix + jobId;
        }
    }
}