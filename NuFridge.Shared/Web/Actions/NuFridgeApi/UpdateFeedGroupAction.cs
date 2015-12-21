using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class UpdateFeedGroupAction : IAction
    {
        private readonly IFeedGroupService _feedGroupService;
        private readonly ILog _log = LogProvider.For<UpdateFeedGroupAction>();

        public UpdateFeedGroupAction(IFeedGroupService feedGroupService)
        {
            _feedGroupService = feedGroupService;
        }


        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanUpdateFeedGroup });

            FeedGroup feedGroup;

            try
            {
                int groupId = int.Parse(parameters.id);

                feedGroup = module.Bind<FeedGroup>();

                if (groupId != feedGroup.Id)
                {
                    return module.Negotiate.WithStatusCode(HttpStatusCode.BadRequest).WithModel("The feed id provided did not match.");
                }

                _feedGroupService.Update(feedGroup);
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return module.Negotiate.WithStatusCode(HttpStatusCode.InternalServerError).WithModel(ex.Message);
            }


            return _feedGroupService.Find(feedGroup.Id);
        }
    }
}