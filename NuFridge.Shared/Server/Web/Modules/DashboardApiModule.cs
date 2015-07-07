using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;
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
