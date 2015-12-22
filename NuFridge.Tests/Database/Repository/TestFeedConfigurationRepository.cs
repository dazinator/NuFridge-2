using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Tests.Database.Repository
{
    public class TestFeedConfigurationRepository : IFeedConfigurationRepository
    {
        private List<FeedConfiguration> _feedConfigurations = new List<FeedConfiguration>(); 

        public void Insert(FeedConfiguration feedConfiguration)
        {
           _feedConfigurations.Add(feedConfiguration);
        }

        public void Delete(FeedConfiguration feedConfiguration)
        {
            _feedConfigurations.Remove(feedConfiguration);
        }

        public FeedConfiguration FindByFeedId(int feedId)
        {
            return _feedConfigurations.FirstOrDefault(fc => fc.FeedId == feedId);
        }

        public void Update(FeedConfiguration feedConfig)
        {
            _feedConfigurations[_feedConfigurations.FindIndex(fd => fd.Id == feedConfig.Id)] = feedConfig;
        }

        public IEnumerable<FeedConfiguration> GetAll()
        {
            return _feedConfigurations;
        }

        public void WithFeedConfigurations(List<FeedConfiguration> feedConfigurations)
        {
            _feedConfigurations = feedConfigurations;
        }
    }
}