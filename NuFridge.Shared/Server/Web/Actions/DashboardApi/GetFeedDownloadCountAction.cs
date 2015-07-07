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
    public class GetFeedDownloadCountAction : IAction
    {
        private readonly IStore _store;

        public GetFeedDownloadCountAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            module.RequiresAuthentication();

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var model = new FeedDownloadCountStatistic(transaction).GetModel();

                return model;
            }
        }
    }
}