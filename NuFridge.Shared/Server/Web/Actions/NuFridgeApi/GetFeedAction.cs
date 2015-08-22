﻿using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
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
            module.RequiresAuthentication();

            int feedId = int.Parse(parameters.id);

            return module.Negotiate.WithModel(_feedService.Find(feedId, false));
        }
    }
}