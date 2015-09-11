using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Web.SignalR
{
    public class ImportPackagesHub : Hub
    {
        public const string GroupPrefix = "ImportPackagesHub:";

        public async Task Subscribe(int jobId)
        {
            await Groups.Add(Context.ConnectionId, GetGroup(jobId));

            var jobService = GlobalHost.DependencyResolver.Resolve<IJobService>();

            Clients.Caller.loadJob(jobService.Find(jobId));
            Clients.Caller.loadDetailedJob(jobService.Find<PackageImportJob>(jobId));
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