﻿using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Scheduler;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DiagnosticsApi
{
    public class GetDiagnosticInformationAction : IAction
    {
        private readonly IJobServer _jobServer;

        public GetDiagnosticInformationAction(IJobServer jobServer)
        {
            _jobServer = jobServer;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            var model = new SystemInformationStatistic(_jobServer).GetModel();
            return model;
        }
    }
}