using Nancy.Security;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class GetFeedConfigurationAction : IAction
    {
        private readonly IStore _store;

        public GetFeedConfigurationAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            module.RequiresAuthentication();

            using (ITransaction transaction = _store.BeginTransaction())
            {
                int feedId = int.Parse(parameters.id);

                return transaction.Query<IFeedConfiguration>().Where("FeedId = @feedId").Parameter("feedId", feedId).First();
            }
        }
    }
}