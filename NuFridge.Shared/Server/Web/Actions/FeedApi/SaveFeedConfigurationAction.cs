using System;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class SaveFeedConfigurationAction : IAction
    {
        private readonly IStore _store;
        private readonly ILog _log = LogProvider.For<SaveFeedConfigurationAction>();

        public SaveFeedConfigurationAction(IStore store)
        {
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            IFeedConfiguration feedConfig;

            try
            {
                int feedId = int.Parse(parameters.id);

                feedConfig = module.Bind<FeedConfiguration>();

                if (feedId != feedConfig.FeedId)
                {
                    return HttpStatusCode.BadRequest;
                }

                ITransaction transaction = _store.BeginTransaction();

                var existingFeedExists = transaction.Query<IFeedConfiguration>().Where("FeedId = @feedId").Parameter("feedId", feedId).Count() > 0;

                if (!existingFeedExists)
                {
                    return HttpStatusCode.NotFound;
                }

                transaction.Update(feedConfig);
                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return HttpStatusCode.InternalServerError;
            }


            return feedConfig;
        }
    }
}
