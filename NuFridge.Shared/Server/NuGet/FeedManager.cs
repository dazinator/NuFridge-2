using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.NuGet
{
    public class FeedManager : IFeedManager
    {
        private readonly IFeedService _feedService;
        private readonly IFeedConfigurationService _feedConfigurationService;
        private readonly IPackageService _packageService;

        private readonly IHomeConfiguration _homeConfiguration;
        private readonly ILog _log = LogProvider.For<FeedManager>();

        public FeedManager(IFeedService feedService, IFeedConfigurationService feedConfigurationService, IHomeConfiguration homeConfiguration, IPackageService packageService)
        {
            _feedService = feedService;
            _feedConfigurationService = feedConfigurationService;
            _homeConfiguration = homeConfiguration;
            _packageService = packageService;
        }

        public void Create(Feed feed)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                _feedService.Insert(feed);

                var appFolder = _homeConfiguration.InstallDirectory;
                var feedFolder = Path.Combine(appFolder, "Feeds", feed.Id.ToString());

                FeedConfiguration config = new FeedConfiguration
                {
                    FeedId = feed.Id,
                    Directory = feedFolder
                };

                _feedConfigurationService.Insert(config);

                transactionScope.Complete();
            }
        }

        public bool Exists(string feedName)
        {
            return _feedService.Exists(feedName);
        }

        public bool Exists(int feedId)
        {
            return _feedService.Exists(feedId);
        }

        public void Delete(int feedId)
        {
            string packageDirectory;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                Feed feed = _feedService.Find(feedId, false);
                _feedService.Delete(feed);

                FeedConfiguration config = _feedConfigurationService.FindByFeedId(feedId);
                packageDirectory = config.Directory;
                _feedConfigurationService.Delete(config);

                IEnumerable<IInternalPackage> packages = _packageService.GetAllPackagesForFeed(feedId).ToList();

                if (packages.Any())
                {
                    _packageService.Delete(packages.Select(pk => pk.PrimaryId));
                }

                transactionScope.Complete();
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
        }
    }

    public interface IFeedManager
    {
        void Create(Feed feed);
        bool Exists(string feedName);
        bool Exists(int feedId);
        void Delete(int feedId);
    }
}