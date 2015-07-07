using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DiagnosticsApi
{
    public class GetDiagnosticInformationAction : IAction
    {
        private readonly IStore _store;

        public GetDiagnosticInformationAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var model = new SystemInformationStatistic(transaction).GetModel();

                return model;
            }
        }
    }
}