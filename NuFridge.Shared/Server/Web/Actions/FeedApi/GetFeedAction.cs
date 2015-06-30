using Nancy;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class GetFeedAction : IAction
    {
        private readonly IStore _store;

        public GetFeedAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
                module.RequiresAuthentication();

                using (ITransaction transaction = _store.BeginTransaction())
                {
                    int feedId = int.Parse(parameters.id);

                    return transaction.Query<IFeed>().Where("Id = @feedId").Parameter("feedId", feedId).First();
                }
        }
    }
}