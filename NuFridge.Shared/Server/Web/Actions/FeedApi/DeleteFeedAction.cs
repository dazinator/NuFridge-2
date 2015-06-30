using System;
using System.IO;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class DeleteFeedAction : IAction
    {
        private readonly IStore _store;

        public DeleteFeedAction(IStore store)
        {
            _store = store;
        }


        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            using (ITransaction transaction = _store.BeginTransaction())
            {
                int feedId = int.Parse(parameters.id);

                var feed = transaction.Query<IFeed>().Where("Id = @feedId").Parameter("feedId", feedId).First();

                if (feed == null)
                {
                    return HttpStatusCode.NotFound;
                }

                var config = transaction.Query<IFeedConfiguration>().Where("FeedId = @feedId").Parameter("feedId", feedId).First();

                transaction.Delete(feed);
                transaction.Delete(config);

                if (Directory.Exists(config.PackagesDirectory))
                {
                    try
                    {
                        Directory.Delete(config.PackagesDirectory);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        //TODO standardized error responses
                        var response = module.Response.AsText(ex.Message);

                        response.StatusCode = HttpStatusCode.InternalServerError;

                        return response;
                    }
                }

                transaction.Commit();
            }

            //TODO responses
            return module.Response.AsJson(new object());
        }
    }
}
