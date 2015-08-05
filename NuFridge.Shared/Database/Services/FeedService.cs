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
    }

    public interface IFeedService
    {
        bool Exists(string feedName);
        void Insert(Feed feed);
    }
}