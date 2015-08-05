using Autofac;
using Nancy;
using NuFridge.Shared.Server.Web.Actions.DashboardApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class DashboardApiModule : NancyModule
    {
        public DashboardApiModule(IContainer container)
        {
            Get["api/dashboard"] = p => container.Resolve<GetDashboardAction>().Execute(p, this);
           Get["api/stats/feedpackagecount"] = p => container.Resolve<GetFeedPackageCountAction>().Execute(p, this);
            Get["api/stats/feeddownloadcount"] = p => container.Resolve<GetFeedDownloadCountAction>().Execute(p, this);
        }
    }
}
