using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Modules;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Scheduler.Jobs.Tasks;
using NuFridge.Shared.Server.Storage;
using NUnit.Framework;

namespace NuFridge.Tests
{
    public class RunPackageRetentionPoliciesTaskTests
    {
        protected Mock<IStore> Store;
        protected Mock<InternalPackageRepositoryFactory> Factory;

        protected List<IFeed> InMemoryFeeds;
        protected List<IFeedConfiguration> InMemoryFeedConfigurations;

        [SetUp]
        public void Setup()
        {
            Store = new Mock<IStore>();
            Factory = new Mock<InternalPackageRepositoryFactory>();

            Mock<IFeed> feed = new Mock<IFeed>();
            feed.Setup(fd => fd.Name).Returns("In Memory Feed");
            feed.Setup(fd => fd.Id).Returns(1);

            InMemoryFeeds = new List<IFeed> { feed.Object };

            Mock<IFeedConfiguration> feedConfig = new Mock<IFeedConfiguration>();
            feedConfig.Setup(fc => fc.FeedId).Returns(1);
            feedConfig.Setup(fc => fc.PackagesDirectory).Returns("Fake directory");
            feedConfig.Setup(fc => fc.MaxReleasePackages).Returns(3);
            feedConfig.Setup(fc => fc.MaxPrereleasePackages).Returns(4);
            feedConfig.Setup(fc => fc.RetentionPolicyEnabled).Returns(true);

            InMemoryFeedConfigurations = new List<IFeedConfiguration> { feedConfig.Object };
        }

        [Test]
        public void T()
        {
            Mock<ITransaction> transaction = new Mock<ITransaction>();
            Store.Setup(st => st.BeginTransaction()).Returns(transaction.Object);

            transaction.Setup(tr => tr.Query<IFeed>().ToList()).Returns(InMemoryFeeds.ToList());
            transaction.Setup(tr => tr.Query<IFeedConfiguration>().ToList()).Returns(InMemoryFeedConfigurations.ToList());
        }

        public class TestableRunPackageRetentionPoliciesTask : RunPackageRetentionPoliciesTask
        {
            public TestableRunPackageRetentionPoliciesTask(IStore store, InternalPackageRepositoryFactory packageRepositoryFactory)
                : base(store, packageRepositoryFactory)
            {

            }
        }
    }
}