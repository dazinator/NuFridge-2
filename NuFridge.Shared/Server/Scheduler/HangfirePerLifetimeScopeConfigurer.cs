using Autofac;
using Hangfire;

namespace NuFridge.Shared.Server.Scheduler
{
    public static class HangfirePerLifetimeScopeConfigurer
    {
        public static void Configure(IContainer container)
        {
            var autofacPerLifetimeScopeJobActivator = new AutofacPerLifetimeScopeJobActivator(container);
            JobActivator.Current = autofacPerLifetimeScopeJobActivator;
            GlobalJobFilters.Filters.Add(new AutofacContainerPerJobFilterAttribute(autofacPerLifetimeScopeJobActivator));
        }
    }
}
