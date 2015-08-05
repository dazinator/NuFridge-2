using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using SimpleCrypto;

namespace NuFridge.Shared.Database.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;

        public FeedService(IFeedRepository feedRepository)
        {
            _feedRepository = feedRepository;
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

        public Feed Find(int feedId)
        {
            return _feedRepository.Find(feedId);
        }
    }

    public interface IFeedService
    {
        void Delete(Feed feed);
        bool Exists(string feedName);
        void Insert(Feed feed);
        bool Exists(int feedId);
        Feed Find(int feedId);
    }
}