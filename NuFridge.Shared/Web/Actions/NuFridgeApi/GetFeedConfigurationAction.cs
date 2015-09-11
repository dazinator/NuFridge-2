using System.Collections.Generic;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class GetFeedConfigurationAction : IAction
    {
        private readonly IFeedConfigurationService _feedConfigurationService;

        public GetFeedConfigurationAction(IFeedConfigurationService feedConfigurationService)
        {
            _feedConfigurationService = feedConfigurationService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanViewFeeds});

            int feedId = int.Parse(parameters.id);

            return _feedConfigurationService.FindByFeedId(feedId);
        }
    }
}