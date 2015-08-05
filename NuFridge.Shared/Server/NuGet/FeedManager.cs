using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Configuration;
using SimpleCrypto;

namespace NuFridge.Shared.Server.NuGet
{
    public class FeedManager : IFeedManager
    {
        private readonly IFeedService _feedService;
        private readonly IFeedConfigurationService _feedConfigurationService;
        private readonly IHomeConfiguration _homeConfiguration;

        public FeedManager(IFeedService feedService, IFeedConfigurationService feedConfigurationService, IHomeConfiguration homeConfiguration)
        {
            _feedService = feedService;
            _feedConfigurationService = feedConfigurationService;
            _homeConfiguration = homeConfiguration;
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
                    Directory = feedFolder,
                    RetentionPolicyDeletePackages = true
                };

                _feedConfigurationService.Insert(config);

                transactionScope.Complete();
            }
        }

        public bool Exists(string feedName)
        {
            return _feedService.Exists(feedName);
        }
    }

    public interface IFeedManager
    {
        void Create(Feed feed);
        bool Exists(string feedName);
    }
}