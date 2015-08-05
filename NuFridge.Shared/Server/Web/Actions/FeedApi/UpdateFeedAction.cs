using System;
using System.Data.Entity;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Logging;
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

            Feed feed;

            try
            {
                int feedId = int.Parse(parameters.id);

                feed = module.Bind<Feed>();

                if (feedId != feed.Id)
                {
                    return HttpStatusCode.BadRequest;
                }

                using (var dbContext = new DatabaseContext())
                {

                    var existingFeed = dbContext.Feeds.AsNoTracking().FirstOrDefault(f => f.Id == feedId);

                    if (existingFeed == null)
                    {
                        return HttpStatusCode.NotFound;
                    }

                    if (!string.IsNullOrWhiteSpace(feed.ApiKey))
                    {
                        ICryptoService cryptoService = new PBKDF2();

                        feed.ApiKeySalt = cryptoService.GenerateSalt();
                        feed.ApiKeyHashed = cryptoService.Compute(feed.ApiKey);
                    }
                    else if (feed.HasApiKey)
                    {
                        feed.ApiKeyHashed = existingFeed.ApiKeyHashed; //Temporary until API Key table is used
                        feed.ApiKeySalt = existingFeed.ApiKeySalt; //Temporary until API Key table is used
                    }

                    dbContext.Feeds.Attach(feed);
                    dbContext.Entry(feed).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
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