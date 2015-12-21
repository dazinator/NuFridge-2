using System.Collections.Generic;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class GetFeedAction : IAction
    {
        private readonly IFeedService _feedService;

        public GetFeedAction(IFeedService feedService)
        {
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanViewFeeds});

            int feedId = int.Parse(parameters.id);

            var feed = _feedService.Find(feedId, false);

            if (feed == null)
            {
                return module.Negotiate.WithStatusCode(HttpStatusCode.NotFound).WithModel("The requested feed was not found.");
            }

            return module.Negotiate.WithModel(feed);
        }
    }
}