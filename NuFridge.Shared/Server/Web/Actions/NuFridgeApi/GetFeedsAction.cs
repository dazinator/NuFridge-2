﻿using System.Collections.Generic;
using System.Net.Security;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Security;

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
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanViewFeeds });

            IEnumerable<FeedGroup> groups = _feedGroupService.GetAll();

            return module.Negotiate.WithModel(groups);
        }
    }
}