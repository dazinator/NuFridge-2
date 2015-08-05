using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Services
{
    public class FeedConfigurationService : IFeedConfigurationService
    {
        private readonly IFeedConfigurationRepository _feedConfigurationRepository;

        public FeedConfigurationService(IFeedConfigurationRepository feedConfigurationRepository)
        {
            _feedConfigurationRepository = feedConfigurationRepository;
        }


        public void Insert(FeedConfiguration feedConfiguration)
        {
           _feedConfigurationRepository.Insert(feedConfiguration);
        }

        public void Delete(FeedConfiguration feedConfiguration)
        {
            _feedConfigurationRepository.Delete(feedConfiguration);
        }

        public FeedConfiguration FindByFeedId(int feedId)
        {
            return _feedConfigurationRepository.FindByFeedId(feedId);
        }
    }

    public interface IFeedConfigurationService
    {
        void Insert(FeedConfiguration feedConfiguration);
        void Delete(FeedConfiguration feedConfiguration);
        FeedConfiguration FindByFeedId(int feedId);
    }
}