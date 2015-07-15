using System;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;
using SimpleCrypto;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class UpdateFeedAction : IAction
    {
        private readonly IStore _store;
        private readonly ILog _log = LogProvider.For<UpdateFeedAction>();

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
                    transaction.Query<IFeed>().Where("Id = @feedId").Parameter("feedId", feedId).First();

                if (existingFeedExists == null)
                {
                    return HttpStatusCode.NotFound;
                }

                if (!string.IsNullOrWhiteSpace(feed.ApiKey))
                {
                    ICryptoService cryptoService = new PBKDF2();

                    feed.ApiKeySalt = cryptoService.GenerateSalt();
                    feed.ApiKeyHashed = cryptoService.Compute(feed.ApiKey);
                }
                else if(feed.HasApiKey)
                {
                    feed.ApiKeyHashed = existingFeedExists.ApiKeyHashed; //Temporary until API Key table is used
                    feed.ApiKeySalt = existingFeedExists.ApiKeySalt; //Temporary until API Key table is used
                }

                transaction.Update(feed);
                transaction.Commit();
                transaction.Dispose();
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return HttpStatusCode.InternalServerError;
            }


            return feed;
        }
    }
}