using System;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class InsertFeedAction : IAction
    {
        private readonly IFeedManager _feedManager;
        private readonly IFeedService _feedService;

        private readonly ILog _log = LogProvider.For<InsertFeedAction>();

        public InsertFeedAction(IFeedManager feedManager, IFeedService feedService)
        {
            _feedManager = feedManager;
            _feedService = feedService;
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

            return _feedService.Find(feed.Id, false);
        }
    }
}