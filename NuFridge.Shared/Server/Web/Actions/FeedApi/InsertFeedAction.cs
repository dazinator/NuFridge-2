using System;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class InsertFeedAction : IAction
    {
        private readonly IFeedManager _feedManager;

        private readonly ILog _log = LogProvider.For<InsertFeedAction>();

        public InsertFeedAction(IFeedManager feedManager)
        {
            _feedManager = feedManager;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            Feed feed;

            try
            {
                feed = module.Bind<Feed>();

                if (_feedManager.Exists(feed.Name))
                {
                    return HttpStatusCode.Conflict;
                }

                _feedManager.Create(feed);
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return HttpStatusCode.InternalServerError;
            }


            feed.ApiKeyHashed = null; //Temporary until API Key table is used
            feed.ApiKeySalt = null; //Temporary until API Key table is used

            return feed;
        }
    }
}