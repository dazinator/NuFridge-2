using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Tests.Database.Repository
{
    public class TestFeedConfigurationRepository : FeedConfigurationRepository
    {
        public List<FeedConfiguration> FeedConfigurations = new List<FeedConfiguration>(); 

        public TestFeedConfigurationRepository(DatabaseContext dbContext) : base(dbContext)
        {
        }

        public override FeedConfiguration FindByFeedId(int feedId)
        {
            return FeedConfigurations.FirstOrDefault(fc => fc.FeedId == feedId);
        }

        public void WithFeedConfigurations(List<FeedConfiguration> feedConfigurations)
        {
            FeedConfigurations = feedConfigurations;
        }
    }
}