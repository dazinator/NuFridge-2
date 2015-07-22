using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Scheduler.Jobs.Tasks
{
    public class ReindexPackagesForFeedTask : ITask
    {
        private readonly IStore _store;

        public ReindexPackagesForFeedTask(IStore store)
        {
            _store = store;
        }

        public void Execute()
        {
        }
    }
}