using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
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
            module.RequiresAuthentication();

            int feedId = int.Parse(parameters.id);

            return _feedConfigurationService.FindByFeedId(feedId);
        }
    }
}