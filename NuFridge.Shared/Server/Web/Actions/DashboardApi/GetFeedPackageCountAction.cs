using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using NuFridge.Shared.Server.Statistics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetFeedPackageCountAction : IAction
    {
        private readonly IStore _store;

        public GetFeedPackageCountAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            module.RequiresAuthentication();

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var model = new FeedPackageCountStatistic(transaction, _store).GetModel();

                return model;
            }
        }
    }
}