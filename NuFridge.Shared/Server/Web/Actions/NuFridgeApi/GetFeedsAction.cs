using System.Collections.Generic;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class GetFeedsAction : IAction
    {
        private readonly IFeedGroupService _feedGroupService;

        public GetFeedsAction(IFeedGroupService feedGroupService)
        {
            _feedGroupService = feedGroupService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            IEnumerable<FeedGroup> groups = _feedGroupService.GetAll();

            return new
            {
                Results = groups
            };
        }
    }
}