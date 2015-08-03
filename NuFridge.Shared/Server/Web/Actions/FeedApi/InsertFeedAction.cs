using System;
using System.IO;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Storage;
using SimpleCrypto;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class InsertFeedAction : IAction
    {
        private readonly IStore _store;
        private readonly IHomeConfiguration _home;
        private readonly ILog _log = LogProvider.For<InsertFeedAction>();

        public InsertFeedAction(IStore store, IHomeConfiguration home)
        {
            _store = store;
            _home = home;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            IFeed feed;

            try
            {
                feed = module.Bind<Feed>();

                ITransaction transaction = _store.BeginTransaction();

                var existingFeedExists =
                    transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feed.Name).Count() >
                    0;

                if (existingFeedExists)
                {
                    return HttpStatusCode.Conflict;
                }

                if (!string.IsNullOrWhiteSpace(feed.ApiKey))
                {
                    ICryptoService cryptoService = new PBKDF2();

                    feed.ApiKeySalt = cryptoService.GenerateSalt();
                    feed.ApiKeyHashed = cryptoService.Compute(feed.ApiKey);
                }

                transaction.Insert(feed);
                transaction.Commit();
                transaction.Dispose();

                transaction = _store.BeginTransaction();

                feed =
                    transaction.Query<IFeed>()
                        .Where("Name = @feedName")
                        .Parameter("feedName", feed.Name)
                        .First();

                var appFolder = _home.InstallDirectory;
                var feedFolder = Path.Combine(appFolder, "Feeds", feed.Id.ToString());

                IFeedConfiguration config = new FeedConfiguration
                {
                    FeedId = feed.Id,
                    Directory = feedFolder, 
                    RpDeletePackages = true
                };

                transaction.Insert(config);

                try
                {
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Dispose();

                    transaction = _store.BeginTransaction();
                    transaction.Delete(feed);
                    transaction.Commit();
                    transaction.Dispose();

                    throw;
                }

                transaction.Dispose();
            }
            catch (Exception ex)
            {
                _log.ErrorException(ex.Message, ex);

                return HttpStatusCode.InternalServerError;
            }

            feed.ApiKeyHashed = null; //Temporary until API Key table is used
            feed.ApiKeySalt = null; //Temporary until API Key table is used

            return feed;
        }
    }
}