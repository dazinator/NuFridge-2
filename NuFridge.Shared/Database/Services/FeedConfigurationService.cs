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
    }

    public interface IFeedConfigurationService
    {
        void Insert(FeedConfiguration feedConfiguration);
    }
}