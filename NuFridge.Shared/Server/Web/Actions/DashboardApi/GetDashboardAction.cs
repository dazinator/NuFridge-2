using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.DashboardApi
{
    public class GetDashboardAction : IAction
    {
        private readonly IStore _store;

        public GetDashboardAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            module.RequiresAuthentication();

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var feedsCount = transaction.Query<IFeed>().Count();
                var usersCount = transaction.Query<User>().Count();
                var packagesCount = transaction.Query<IInternalPackage>().Count();

                return new
                {
                    feedCount = feedsCount,
                    userCount = usersCount,
                    packageCount = packagesCount
                };
            }
        }
    }
}