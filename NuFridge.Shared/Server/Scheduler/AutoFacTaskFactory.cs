using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentScheduler;

namespace NuFridge.Shared.Server.Scheduler
{
    public class AutofacTaskFactory : ITaskFactory
    {
        private IContainer Container { get; set; }

        public AutofacTaskFactory(IContainer container)
        {
            Container = container;
        }

        public ITask GetTaskInstance<T>() where T : ITask
        {
            return Container.Resolve<T>();
        }
    }
}