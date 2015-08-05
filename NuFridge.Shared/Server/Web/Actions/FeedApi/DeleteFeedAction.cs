using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.FeedApi
{
    public class DeleteFeedAction : IAction
    {
        private readonly IFeedManager _feedManager;
        private readonly ILog _log = LogProvider.For<DeleteFeedAction>();

        public DeleteFeedAction(IFeedManager feedManager)
        {
            _feedManager = feedManager;
        }


        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAuthentication();

            int feedId = int.Parse(parameters.id);

            if (!_feedManager.Exists(feedId))
            {
                return HttpStatusCode.NotFound;
            }

            _feedManager.Delete(feedId);

            Feed feed;
            FeedConfiguration config;
            List<InternalPackage> packages;

            using (var dbContext = new DatabaseContext())
            {
                feed = dbContext.Feeds.FirstOrDefault(fd => fd.Id == feedId);
            }

            if (feed == null)
            {
                return HttpStatusCode.NotFound;
            }

            using (var dbContext = new DatabaseContext())
            {
                config = dbContext.FeedConfigurations.FirstOrDefault(fc => fc.FeedId == feedId);
            }

            string packageDirectory = config.Directory;

            using (var dbContext = new DatabaseContext())
            {
                packages = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + feedId).Where(pk => pk.FeedId == feedId).ToList();

                foreach (var package in packages)
                {
                    dbContext.Packages.Remove(package);
                }

                dbContext.Feeds.Remove(feed);
                dbContext.FeedConfigurations.Remove(config);

                dbContext.SaveChanges();
            }

            if (Directory.Exists(packageDirectory))
            {
                try
                {
                    Directory.Delete(packageDirectory, true);
                }
                catch (Exception ex)
                {
                    _log.ErrorException(ex.Message, ex);
                }
            }

            return module.Response.AsJson(new object());
        }
    }
}
