using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.Modules;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Scheduler.Jobs;
using NuFridge.Shared.Server.Storage;
using NuGet;
using NUnit.Framework;

namespace NuFridge.Tests
{
    public class RunPackageRetentionPoliciesTaskTests
    {
        protected Mock<IStore> Store;
        protected Mock<IInternalPackageRepositoryFactory> PackageRepoFactory;
        protected Mock<IInternalPackageRepository> PackageRepo;

        protected List<IFeed> InMemoryFeeds;
        protected List<IFeedConfiguration> InMemoryFeedConfigurations;

        [SetUp]
        public void Setup()
        {
            Store = new Mock<IStore>();
            PackageRepoFactory = new Mock<IInternalPackageRepositoryFactory>();
            PackageRepo = new Mock<IInternalPackageRepository>();
            PackageRepoFactory.Setup(fc => fc.Create(It.IsAny<Int32>())).Returns(PackageRepo.Object);
        }

        [Test]
        public void PolicyNotEnabled()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);

            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0"));
            mock.SetupProperty(fc => fc.Version, "1.0.0");

            Mock<IInternalPackage> mock2 = new Mock<IInternalPackage>();

            mock2.SetupProperty(fc => fc.Title, "Test Package");
            mock2.SetupProperty(fc => fc.PrimaryId, 1);

            mock2.SetupProperty(fc => fc.Id, "TestPackage");
            mock2.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.1"));
            mock2.SetupProperty(fc => fc.Version, "1.0.1");

            SetupMockObjects(1, 1, new List<Mock<IInternalPackage>> {mock, mock2}, false);

            TestableRunPackageRetentionPoliciesTask task = new TestableRunPackageRetentionPoliciesTask(Store.Object, PackageRepoFactory.Object);

            //task.Execute();

            PackageRepoFactory.Verify(fc => fc.Create(It.IsAny<Int32>()), Times.Never);
            PackageRepo.Verify(pr => pr.RemovePackage(It.IsAny<IInternalPackage>()), Times.Never);
        }

        [Test]
        public void NoPackages()
        {
            SetupMockObjects(1, 1, new List<Mock<IInternalPackage>>(), true);

            TestableRunPackageRetentionPoliciesTask task = new TestableRunPackageRetentionPoliciesTask(Store.Object, PackageRepoFactory.Object);

            //task.Execute();

            PackageRepoFactory.Verify(fc => fc.Create(It.IsAny<Int32>()), Times.Never);
            PackageRepo.Verify(pr => pr.RemovePackage(It.IsAny<IInternalPackage>()), Times.Never);
        }

        [Test]
        public void MultiplePrereleasePackagesDeleted()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);
      
            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0-alpha"));
            mock.SetupProperty(fc => fc.Version, "1.0.0-alpha");

            Mock<IInternalPackage> mock2 = new Mock<IInternalPackage>();

            mock2.SetupProperty(fc => fc.Title, "Test Package");
            mock2.SetupProperty(fc => fc.PrimaryId, 1);

            mock2.SetupProperty(fc => fc.Id, "TestPackage");
            mock2.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.1-alpha"));
            mock2.SetupProperty(fc => fc.Version, "1.0.1-alpha");

            Mock<IInternalPackage> mock3 = new Mock<IInternalPackage>();

            mock3.SetupProperty(fc => fc.Title, "Test Package");
            mock3.SetupProperty(fc => fc.PrimaryId, 1);

            mock3.SetupProperty(fc => fc.Id, "TestPackage");
            mock3.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.2-alpha"));
            mock3.SetupProperty(fc => fc.Version, "1.0.2-alpha");

            SetupMockObjects(1, 1, new List<Mock<IInternalPackage>> { mock, mock2, mock3 }, true);

            TestableRunPackageRetentionPoliciesTask task = new TestableRunPackageRetentionPoliciesTask(Store.Object, PackageRepoFactory.Object);

            //task.Execute();

            PackageRepoFactory.Verify(fc => fc.Create(It.IsAny<Int32>()), Times.Once);
            PackageRepo.Verify(pr => pr.RemovePackage(mock.Object), Times.Once);
            PackageRepo.Verify(pr => pr.RemovePackage(mock2.Object), Times.Once);
            PackageRepo.Verify(pr => pr.RemovePackage(mock3.Object), Times.Never);
        }

        [Test]
        public void MultipleReleasePackagesDeleted()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);
          
            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0"));
            mock.SetupProperty(fc => fc.Version, "1.0.0");

            Mock<IInternalPackage> mock2 = new Mock<IInternalPackage>();

            mock2.SetupProperty(fc => fc.Title, "Test Package");
            mock2.SetupProperty(fc => fc.PrimaryId, 1);
 
            mock2.SetupProperty(fc => fc.Id, "TestPackage");
            mock2.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.1"));
            mock2.SetupProperty(fc => fc.Version, "1.0.1");

            SetupMockObjects(1, 1, new List<Mock<IInternalPackage>> { mock, mock2 }, true);

            TestableRunPackageRetentionPoliciesTask task = new TestableRunPackageRetentionPoliciesTask(Store.Object, PackageRepoFactory.Object);

            //task.Execute();

            PackageRepoFactory.Verify(fc => fc.Create(It.IsAny<Int32>()), Times.Once);
            PackageRepo.Verify(pr => pr.RemovePackage(mock.Object), Times.Once);
            PackageRepo.Verify(pr => pr.RemovePackage(mock2.Object), Times.Never);
        }

        [Test]
        public void SingleReleasePackageNotDeleted()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);
       
            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0"));
            mock.SetupProperty(fc => fc.Version, "1.0.0");

            SetupMockObjects(1, 1, new List<Mock<IInternalPackage>> { mock }, true);

            TestableRunPackageRetentionPoliciesTask task = new TestableRunPackageRetentionPoliciesTask(Store.Object, PackageRepoFactory.Object);

            //task.Execute();

            PackageRepoFactory.Verify(fc => fc.Create(It.IsAny<Int32>()), Times.Never);
        }

        [Test]
        public void SinglePrereleasePackageNotDeleted()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);
            
            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.SetupProperty(fc => fc.IsPrerelease, true);
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0-alpha"));
            mock.SetupProperty(fc => fc.Version, "1.0.0-alpha");

            SetupMockObjects(1, 1, new List<Mock<IInternalPackage>> { mock }, true);

            TestableRunPackageRetentionPoliciesTask task = new TestableRunPackageRetentionPoliciesTask(Store.Object, PackageRepoFactory.Object);

            //task.Execute();

            PackageRepoFactory.Verify(fc => fc.Create(It.IsAny<Int32>()), Times.Never);
        }

        private void SetupMockObjects(int maxReleasePackages, int maxPrereleasePackages, List<Mock<IInternalPackage>> packageMocks, bool retentionPolicyEnabled)
        {
            InMemoryFeeds = new List<IFeed>();
            InMemoryFeedConfigurations = new List<IFeedConfiguration>();

            Mock<IFeed> feed = new Mock<IFeed>();
            feed.Setup(fd => fd.Name).Returns("In Memory Feed");
            feed.Setup(fd => fd.Id).Returns(1);
            InMemoryFeeds.Add(feed.Object);

            Mock<IFeedConfiguration> feedConfig = new Mock<IFeedConfiguration>();
            feedConfig.Setup(fc => fc.FeedId).Returns(1);
            feedConfig.Setup(fc => fc.Directory).Returns("Fake directory");
            feedConfig.Setup(fc => fc.MaxReleasePackages).Returns(maxReleasePackages);
            feedConfig.Setup(fc => fc.MaxPrereleasePackages).Returns(maxPrereleasePackages);
            feedConfig.Setup(fc => fc.RetentionPolicyEnabled).Returns(retentionPolicyEnabled);
            InMemoryFeedConfigurations.Add(feedConfig.Object);

            //Mock<ITransaction> transaction = new Mock<ITransaction>();
            //Store.Setup(st => st.BeginTransaction()).Returns(transaction.Object);

            //transaction.Setup(tr => tr.Query<IFeed>().ToList()).Returns(InMemoryFeeds.ToList());
            //transaction.Setup(tr => tr.Query<IFeedConfiguration>().ToList()).Returns(InMemoryFeedConfigurations.ToList());

            //Mock<IQueryBuilder<IInternalPackage>> queryMock = new Mock<IQueryBuilder<IInternalPackage>>();
            //transaction.Setup(tr => tr.Query<IInternalPackage>()).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.Where(It.IsAny<string>())).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.Parameter(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.LikeParameter(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.OrderBy(It.IsAny<string>())).Returns(queryMock.Object);

            //queryMock.Setup(qu => qu.ToList()).Returns(packageMocks.Select(pm => pm.Object).ToList());
            //queryMock.Setup(qu => qu.First()).Returns(packageMocks.FirstOrDefault() != null ? packageMocks.First().Object : null);
            //queryMock.Setup(qu => qu.Count()).Returns(packageMocks.Count());
        }

        public class TestableRunPackageRetentionPoliciesTask : RunPackageRetentionPoliciesJob
        {
            public TestableRunPackageRetentionPoliciesTask(IStore store, IInternalPackageRepositoryFactory packageRepositoryFactory)
                : base(store, null, null, null, null)
            {

            }

            protected override bool IsPackageDirectoryValid(string path)
            {
                return true;
            }
        }
    }
}