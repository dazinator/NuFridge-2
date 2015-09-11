﻿using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class SaveFeedConfigurationAction : IAction
    {
        private readonly IFeedConfigurationService _feedConfigurationService;
        private readonly ILog _log = LogProvider.For<SaveFeedConfigurationAction>();

        public SaveFeedConfigurationAction(IFeedConfigurationService feedConfigurationService)
        {
            _feedConfigurationService = feedConfigurationService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanUpdateFeed });

            FeedConfiguration feedConfig;

            try
            {
                int feedId = int.Parse(parameters.id);

                feedConfig = module.Bind<FeedConfiguration>();

                if (feedId != feedConfig.FeedId)
                {
                    return module.Negotiate.WithStatusCode(HttpStatusCode.BadRequest).WithModel("Feed id does not match.");
                }

                _feedConfigurationService.Update(feedConfig);

            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return module.Negotiate.WithStatusCode(HttpStatusCode.InternalServerError).WithModel(ex.Message);
            }

            return feedConfig;
        }
    }
}