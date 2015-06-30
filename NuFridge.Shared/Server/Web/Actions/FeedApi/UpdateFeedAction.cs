using System;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class UpdateFeedAction : IAction
    {
        private readonly IStore _store;

        public UpdateFeedAction(IStore store)
        {
            _store = store;
        }


        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            IFeed feed;

            try
            {
                int feedId = int.Parse(parameters.id);

                feed = module.Bind<Feed>();

                if (feedId != feed.Id)
                {
                    return HttpStatusCode.BadRequest;
                }

                ITransaction transaction = _store.BeginTransaction();

                var existingFeedExists =
                    transaction.Query<IFeed>().Where("Id = @feedId").Parameter("feedId", feedId).Count() >
                    0;

                if (!existingFeedExists)
                {
                    return HttpStatusCode.NotFound;
                }

                transaction.Update(feed);
                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                return HttpStatusCode.InternalServerError;
            }


            return feed;
        }
    }
}