using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;
using Nancy.Bootstrappers.Autofac;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs.Tasks
{
    public class UpdateFeedPackageCountStatisticTask : ITask
    {
        private IStore Store { get; set; }

        public UpdateFeedPackageCountStatisticTask(IStore store)
        {
            Store = store;
        }

        public void Execute()
        {
            using (ITransaction transaction = Store.BeginTransaction())
            {
                FeedPackageCountStatistic stat = new FeedPackageCountStatistic(transaction);

                stat.UpdateModel();
            }
        }
    }
}