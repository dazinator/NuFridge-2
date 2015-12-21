using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class DeleteFeedGroupAction : IAction
    {
        private readonly IFeedGroupService _feedGroupService;

        public DeleteFeedGroupAction(IFeedGroupService feedGroupService)
        {
            _feedGroupService = feedGroupService;
        }


        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanDeleteFeedGroup });

            int groupId = int.Parse(parameters.id);

            var feedGroup = _feedGroupService.Find(groupId);

            if (feedGroup == null)
            {
                return module.Negotiate.WithStatusCode(HttpStatusCode.NotFound);
            }

            if (feedGroup.Feeds.Any())
            {
                return module.Negotiate.WithStatusCode(HttpStatusCode.Conflict).WithModel("Groups which contain active feeds can not be deleted.");
            }

            _feedGroupService.Delete(feedGroup);

            return module.Negotiate.WithModel(new object());
        }
    }
}