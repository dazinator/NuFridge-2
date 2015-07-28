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

        public async Task Subscribe(int feedId)
        {
            await Groups.Add(Context.ConnectionId, GetGroup(feedId));
        }


        public static string GetGroup(int feedId)
        {
            return GroupPrefix + feedId;
        }
    }
}