using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.NuGet;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
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
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanInsertFeed});

            Feed feed;

            try
            {
                feed = module.Bind<Feed>();

                if (_feedManager.Exists(feed.Name))
                {
                    return
                        module.Negotiate.WithStatusCode(HttpStatusCode.Conflict)
                            .WithModel($"A feed with the name '{feed.Name}' already exists.");
                }

                _feedManager.Create(feed);
            }
            catch (FeedConflictException ex)
            {
                _log.ErrorException(ex.Message, ex);

                return module.Negotiate.WithStatusCode(HttpStatusCode.Conflict).WithModel(ex.Message);
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return module.Negotiate.WithStatusCode(HttpStatusCode.InternalServerError).WithModel(ex.Message);
            }

            return _feedService.Find(feed.Id, false);
        }
    }
}