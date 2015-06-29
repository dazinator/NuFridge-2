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
using NuGet;
using NUnit.Framework;

namespace NuFridge.Tests
{
    public class RunPackageRetentionPoliciesTaskTests
    {
        protected Mock<IStore> Store;
        protected InternalPackageRepositoryFactory Factory;

        protected List<IFeed> InMemoryFeeds;
        protected List<IFeedConfiguration> InMemoryFeedConfigurations;

        private IContainer _container;


        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new NuGetModule());
            _container = builder.Build();

            Store = new Mock<IStore>();
            Factory = _container.Resolve<InternalPackageRepositoryFactory>();

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

            Mock<IQueryBuilder<IInternalPackage>> queryMock = new Mock<IQueryBuilder<IInternalPackage>>();
            transaction.Setup(tr => tr.Query<IInternalPackage>()).Returns(queryMock.Object);
            queryMock.Setup(qu => qu.Where(It.IsAny<string>())).Returns(queryMock.Object);
            queryMock.Setup(qu => qu.Parameter(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
            queryMock.Setup(qu => qu.LikeParameter(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
            queryMock.Setup(qu => qu.OrderBy(It.IsAny<string>())).Returns(queryMock.Object);

            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.Id, 1);
            mock.SetupProperty(fc => fc.FeedId, 1);

            mock.Object.SetSemanticVersion(SemanticVersion.Parse("1.0.0"));

            queryMock.Setup(qu => qu.ToList()).Returns(new List<IInternalPackage> { mock.Object });
            queryMock.Setup(qu => qu.First()).Returns(mock.Object);
            queryMock.Setup(qu => qu.Count()).Returns(1);

            TestableRunPackageRetentionPoliciesTask task = new TestableRunPackageRetentionPoliciesTask(Store.Object, Factory);

            task.Execute();
        }

        public class TestableRunPackageRetentionPoliciesTask : RunPackageRetentionPoliciesTask
        {
            public TestableRunPackageRetentionPoliciesTask(IStore store, InternalPackageRepositoryFactory packageRepositoryFactory)
                : base(store, packageRepositoryFactory)
            {

            }

            protected override bool IsPackageDirectoryValid(string path)
            {
                return true;
            }
        }
    }
}