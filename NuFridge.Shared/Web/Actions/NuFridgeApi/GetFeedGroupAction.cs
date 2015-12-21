using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class GetFeedGroupAction : IAction
    {
        private readonly IFeedGroupService _feedGroupService;

        public GetFeedGroupAction(IFeedGroupService feedGroupService)
        {
            this._feedGroupService = feedGroupService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            int groupId = int.Parse(parameters.id);

            FeedGroup feedGroup = _feedGroupService.Find(groupId);

            return module.Negotiate.WithModel(feedGroup);
        }
    }
}