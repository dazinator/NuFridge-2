using System;
using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Server.Configuration;
using SimpleCrypto;

namespace NuFridge.Shared.Database.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;
        private readonly IHomeConfiguration _homeConfiguration;

        public FeedService(IFeedRepository feedRepository, IHomeConfiguration homeConfiguration)
        {
            _feedRepository = feedRepository;
            _homeConfiguration = homeConfiguration;
        }

        public void Delete(Feed feed)
        {
            _feedRepository.Delete(feed);
        }

        public bool Exists(string feedName)
        {
            IEnumerable<Feed> feeds = _feedRepository.GetAll();

            return feeds.Any(feed => feed.Name.Equals(feedName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Insert(Feed feed)
        {
            if (!string.IsNullOrWhiteSpace(feed.ApiKey))
            {
                ICryptoService cryptoService = new PBKDF2();

                feed.ApiKeySalt = cryptoService.GenerateSalt();
                feed.ApiKeyHashed = cryptoService.Compute(feed.ApiKey);
            }

            _feedRepository.Insert(feed);
        }

        public bool Exists(int feedId)
        {
            return _feedRepository.Find(feedId) != null;
        }

        public IEnumerable<Feed> GetAll()
        {
            return _feedRepository.GetAll();
        }

        public IEnumerable<Feed> GetAllPaged(int pageNumber, int rowsPerPage, bool includeApiKey)
        {
            var feeds = _feedRepository.GetAllPaged(pageNumber, rowsPerPage).ToList();

            foreach (var feed in feeds)
            {
                ConfigureFeedEntity(feed, includeApiKey);
            }

            return feeds;
        }

        private void ConfigureFeedEntity(Feed feed, bool includeApiKey)
        {
            if (!string.IsNullOrWhiteSpace(feed.ApiKeyHashed))
            {
                feed.HasApiKey = true;
            }

            if (!includeApiKey)
            {
                feed.ApiKeyHashed = null; //We don't want to expose this to the front end
                feed.ApiKeySalt = null; //We don't want to expose this to the front end
            }

            feed.RootUrl = $"{_homeConfiguration.ListenPrefixes}{(_homeConfiguration.ListenPrefixes.EndsWith("/") ? "" : "/")}feeds/{feed.Name}";
        }


        public Feed Find(string feedName, bool includeApiKey)
        {
            var feed = _feedRepository.Find(feedName);

            if (feed != null)
            {
                ConfigureFeedEntity(feed, includeApiKey);
            }

            return feed;
        }

        public Feed Find(int feedId, bool includeApiKey)
        {
            var feed = _feedRepository.Find(feedId);

            if (feed != null)
            {
                ConfigureFeedEntity(feed, includeApiKey);
            }

            return feed;
        }

        public IEnumerable<Feed> Search(string name)
        {
            return _feedRepository.Search(name);
        }

        public int GetCount()
        {
            return _feedRepository.GetCount();
        }

        public void Update(Feed feed)
        {
            var existingFeed = Find(feed.Id, true);

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

            _feedRepository.Update(feed);
        }
    }

    public interface IFeedService
    {
        void Delete(Feed feed);
        bool Exists(string feedName);
        void Insert(Feed feed);
        bool Exists(int feedId);
        IEnumerable<Feed> GetAll();
        IEnumerable<Feed> GetAllPaged(int pageNumber, int rowsPerPage, bool includeApiKey);
        Feed Find(int feedId, bool includeApiKey);
        Feed Find(string feedName, bool includeApiKey);
        IEnumerable<Feed> Search(string name);
        int GetCount();
        void Update(Feed feed);
    }
}