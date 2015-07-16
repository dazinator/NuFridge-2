using Nancy;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class GetFeedAction : IAction
    {
        private readonly IStore _store;
        private readonly IHomeConfiguration _config;

        public GetFeedAction(IStore store, IHomeConfiguration config)
        {
            _store = store;
            _config = config;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
                module.RequiresAuthentication();

                using (ITransaction transaction = _store.BeginTransaction())
                {
                    int feedId = int.Parse(parameters.id);

                    var feed = transaction.Query<IFeed>().Where("Id = @feedId").Parameter("feedId", feedId).First();
                    if (feed != null)
                    {
                        //Temporary until the API Key table is used
                        if (!string.IsNullOrWhiteSpace(feed.ApiKeyHashed))
                        {
                            feed.HasApiKey = true;
                        }
                        feed.ApiKeyHashed = null; //We don't want to expose this to the front end
                        feed.ApiKeySalt = null; //We don't want to expose this to the front end

                        bool endsWithSlash = _config.ListenPrefixes.EndsWith("/");

                        var baseAddress = string.Format("{0}{1}feeds/{2}", _config.ListenPrefixes, endsWithSlash ? "" : "/", feed.Name);

                        feed.RootUrl = baseAddress;
                    }

                    return feed;
                }
        }
    }
}