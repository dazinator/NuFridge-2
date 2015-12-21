using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.NuGet;
using NuFridge.Shared.Security;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class InsertFeedGroupAction : IAction
    {
        private readonly IFeedGroupService _feedGroupService;
        private readonly ILog _log = LogProvider.For<InsertFeedGroupAction>();

        public InsertFeedGroupAction(IFeedGroupService feedGroupService)
        {
            _feedGroupService = feedGroupService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanInsertFeedGroup});

            FeedGroup feedGroup;

            try
            {
                feedGroup = module.Bind<FeedGroup>();

                if (_feedGroupService.Exists(feedGroup.Name))
                {
                    return
                        module.Negotiate.WithStatusCode(HttpStatusCode.Conflict)
                            .WithModel($"A feed group with the name '{feedGroup.Name}' already exists.");
                }

                _feedGroupService.Insert(feedGroup);
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