using System;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class UpdateFeedAction : IAction
    {
        private readonly IFeedService _feedService;
        private readonly ILog _log = LogProvider.For<UpdateFeedAction>();

        public UpdateFeedAction(IFeedService feedService)
        {
            _feedService = feedService;
        }


        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            Feed feed;

            try
            {
                int feedId = int.Parse(parameters.id);

                feed = module.Bind<Feed>();

                if (feedId != feed.Id)
                {
                    return HttpStatusCode.BadRequest;
                }

                _feedService.Update(feed);
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