using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Server;

namespace NuFridge.Shared.Server.Scheduler
{
    public class AutofacContainerPerJobFilterAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly AutofacPerLifetimeScopeJobActivator _autofacPerLifetimeScopeJobActivator;

        public AutofacContainerPerJobFilterAttribute(AutofacPerLifetimeScopeJobActivator autofacPerLifetimeScopeJobActivator)
        {
            _autofacPerLifetimeScopeJobActivator = autofacPerLifetimeScopeJobActivator;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            _autofacPerLifetimeScopeJobActivator.DisposeJobLifetimeScope();
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            _autofacPerLifetimeScopeJobActivator.CreateJobLifetimeScope();
        }
    }
}
