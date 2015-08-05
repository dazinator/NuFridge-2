using Hangfire;
using Hangfire.Storage;
using Nancy;
using Nancy.Security;

namespace NuFridge.Shared.Server.Web.Actions.SchedulerApi
{
    class GetServersAction : IAction
    {
        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();

            var servers = monitoringApi.Servers();

            return new
            {
                Results = servers
            };
        }
    }
}
