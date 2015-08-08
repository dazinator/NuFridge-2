using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Scheduler;
using NuFridge.Shared.Server.Statistics;

namespace NuFridge.Shared.Server.Web.Actions.DiagnosticsApi
{
    public class GetDiagnosticInformationAction : IAction
    {
        private readonly IJobServerManager _jobServerManager;
        private readonly IStatisticService _statisticService;

        public GetDiagnosticInformationAction(IJobServerManager jobServerManager, IStatisticService statisticService)
        {
            _jobServerManager = jobServerManager;
            _statisticService = statisticService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var model = new SystemInformationStatistic(_jobServerManager, _statisticService).GetModel();
            return model;
        }
    }
}