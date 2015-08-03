using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace NuFridge.Shared.Server.Web.SignalR
{
    public class ImportPackagesHub : Hub
    {
        public const string GroupPrefix = "ImportPackagesHub:";

        public async Task Subscribe(string jobId)
        {
            await Groups.Add(Context.ConnectionId, GetGroup(jobId));
        }


        public static string GetGroup(string jobId)
        {
            return GroupPrefix + jobId;
        }
    }
}