using Nancy;
using Nancy.Security;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DiagnosticsApi
{
    public class GetDiagnosticInformationAction : IAction
    {
        private readonly IStore _store;
        private readonly IServerEngine _engine;

        public GetDiagnosticInformationAction(IStore store, IServerEngine engine)
        {
            _store = store;
            _engine = engine;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var model = new SystemInformationStatistic(transaction, _engine).GetModel();

                return model;
            }
        }
    }
}