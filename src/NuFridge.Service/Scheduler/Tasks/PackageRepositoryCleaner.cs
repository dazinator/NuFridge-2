using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentScheduler;
using NuFridge.Service.Feeds;

namespace NuFridge.Service.Scheduler.Tasks
{
    public class PackageRepositoryCleaner : ITask
    {
        public void Execute()
        {
            var feedManager = FeedManager.Instance();

            foreach (var feed in feedManager.RunningFeeds)
            {

            }
        }
    }
}