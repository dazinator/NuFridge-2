using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using NuFridge.Shared.Server.Web.Actions.DiagnosticsApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class DiagnosticsApiModule : NancyModule
    {
        public DiagnosticsApiModule(IContainer container)
        {
            Get["api/diagnostics"] = p => container.Resolve<GetDiagnosticInformationAction>().Execute(p, this);
        }
    }
}