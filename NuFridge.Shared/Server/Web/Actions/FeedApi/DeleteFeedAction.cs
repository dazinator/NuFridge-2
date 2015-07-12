using System;
using System.Collections.Generic;
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


            int feedId = int.Parse(parameters.id);

            IFeed feed;
            IFeedConfiguration config;
            List<IInternalPackage> packages;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                feed = transaction.Query<IFeed>()
                    .Where("Id = @feedId")
                    .Parameter("feedId", feedId)
                    .First();
            }

            if (feed == null)
            {
                return HttpStatusCode.NotFound;
            }

            using (ITransaction transaction = _store.BeginTransaction())
            {
                config =
                    transaction.Query<IFeedConfiguration>()
                        .Where("FeedId = @feedId")
                        .Parameter("feedId", feedId)
                        .First();
            }

            using (ITransaction transaction = _store.BeginTransaction())
            {
                packages =
                    transaction.Query<IInternalPackage>()
                    .Where("FeedId = @feedId")
                    .Parameter("feedId", feedId)
                    .ToList();
            }

            string packageDirectory = config.Directory;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                foreach (var package in packages)
                {
                    transaction.Delete(package);
                }

                transaction.Delete(feed);
                transaction.Delete(config);

                transaction.Commit();
            }

            if (Directory.Exists(packageDirectory))
            {
                try
                {
                    Directory.Delete(packageDirectory, true);
                }
                catch (Exception ex)
                {

                }
            }

            return module.Response.AsJson(new object());
        }
    }
}
