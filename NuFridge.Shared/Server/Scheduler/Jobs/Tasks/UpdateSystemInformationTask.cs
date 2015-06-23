using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;
using NuFridge.Shared.Server.Diagnostics;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs.Tasks
{
    public class UpdateSystemInformationTask : ITask
    {
        private IStore Store { get; set; }

        public UpdateSystemInformationTask(IStore store)
        {
            Store = store;
        }

        public void Execute()
        {
            using (ITransaction transaction = Store.BeginTransaction())
            {
                SystemInformationStatistic stat = new SystemInformationStatistic(transaction);

                stat.UpdateModel();
            }
        }
    }
}
