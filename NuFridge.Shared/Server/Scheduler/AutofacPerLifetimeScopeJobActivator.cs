using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hangfire;

namespace NuFridge.Shared.Server.Scheduler
{
    public class AutofacPerLifetimeScopeJobActivator : JobActivator
    {
        private static IContainer _mainContainer;

        [ThreadStatic]
        private static ILifetimeScope _childLifetimeScope;

        public AutofacPerLifetimeScopeJobActivator(IContainer container)
        {
            _mainContainer = container;
        }

        public override object ActivateJob(Type jobType)
        {
            return _childLifetimeScope.Resolve(jobType);
        }

        public void CreateJobLifetimeScope()
        {
            _childLifetimeScope = _mainContainer.BeginLifetimeScope();
        }

        public void DisposeJobLifetimeScope()
        {
            _childLifetimeScope.Dispose();
            _childLifetimeScope = null;
        }
    }
}
