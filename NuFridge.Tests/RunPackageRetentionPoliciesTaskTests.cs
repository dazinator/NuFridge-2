using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using Moq;
using NuFridge.Shared.Application;
using NuFridge.Shared.Autofac;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.NuGet.Repository;
using NuFridge.Shared.Scheduler.Jobs.Definitions;
using NuFridge.Tests.Autofac;
using NuFridge.Tests.Scheduler.Jobs.Definitions;
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

        private IContainer _container;

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void PolicyNotEnabled()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);
            mock.SetupProperty(fc => fc.FeedId, 1);
            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.SetupProperty(fc => fc.IsAbsoluteLatestVersion, false);
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0-alpha"));
            mock.SetupProperty(fc => fc.Version, "1.0.0-alpha");
            mock.SetupProperty(fc => fc.IsPrerelease, true);

            Mock<IInternalPackage> mock2 = new Mock<IInternalPackage>();

            mock2.SetupProperty(fc => fc.Title, "Test Package");
            mock2.SetupProperty(fc => fc.PrimaryId, 1);
            mock2.SetupProperty(fc => fc.FeedId, 1);
            mock2.SetupProperty(fc => fc.Id, "TestPackage");
            mock2.SetupProperty(fc => fc.IsAbsoluteLatestVersion, false);
            mock2.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.1-alpha"));
            mock2.SetupProperty(fc => fc.Version, "1.0.1-alpha");
            mock2.SetupProperty(fc => fc.IsPrerelease, true);

            Mock<IInternalPackage> mock3 = new Mock<IInternalPackage>();

            mock3.SetupProperty(fc => fc.Title, "Test Package");
            mock3.SetupProperty(fc => fc.PrimaryId, 1);
            mock3.SetupProperty(fc => fc.IsAbsoluteLatestVersion, true);
            mock3.SetupProperty(fc => fc.FeedId, 1);
            mock3.SetupProperty(fc => fc.Id, "TestPackage");
            mock3.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.2-alpha"));
            mock3.SetupProperty(fc => fc.Version, "1.0.2-alpha");
            mock3.SetupProperty(fc => fc.IsPrerelease, true);

            var builder = new ContainerBuilder();
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterModule(new TestableConfigurationModule());
            builder.RegisterModule(new PortalModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new TestableSchedulerModule());
            builder.RegisterModule(new NuGetModule());
            builder.RegisterModule(
                new TestableDatabaseModule()
                    .WithFeeds(new List<Feed>
                    {
                        new Feed {Id = 1, Name = "Test", GroupId = 1}
                    })
                    .WithFeedConfigurations(new List<FeedConfiguration>
                    {
                        new FeedConfiguration {FeedId = 1, Directory = "", RetentionPolicyEnabled = false, MaxPrereleasePackages = 2}
                    })
                    .WithPackages(new List<IInternalPackage>
                    {
                        mock.Object, mock2.Object, mock3.Object
                    }));

            _container = builder.Build();

            TestRunPackageRetentionPoliciesJob task = _container.Resolve<TestRunPackageRetentionPoliciesJob>();
            task.Execute(new JobCancellationToken(false));

            var repo = _container.Resolve<Shared.Database.Repository.IPackageRepository>();

            Assert.AreEqual(3, repo.GetCount(mock.Object.FeedId));
            Assert.Contains(mock.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock.Object.FeedId).ToList()));
            Assert.Contains(mock2.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock2.Object.FeedId).ToList()));
            Assert.Contains(mock3.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock3.Object.FeedId).ToList()));
        }

        [Test]
        public void NoPackages()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterModule(new TestableConfigurationModule());
            builder.RegisterModule(new PortalModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new TestableSchedulerModule());
            builder.RegisterModule(new NuGetModule());
            builder.RegisterModule(
                new TestableDatabaseModule()
                    .WithFeeds(new List<Feed>
                    {
                        new Feed {Id = 1, Name = "Test", GroupId = 1}
                    })
                    .WithFeedConfigurations(new List<FeedConfiguration>
                    {
                        new FeedConfiguration {FeedId = 1, Directory = "", RetentionPolicyEnabled = true, MaxPrereleasePackages = 2}
                    })
                    .WithPackages(new List<IInternalPackage>()));

            _container = builder.Build();

            TestRunPackageRetentionPoliciesJob task = _container.Resolve<TestRunPackageRetentionPoliciesJob>();
            task.Execute(new JobCancellationToken(false));

            var repo = _container.Resolve<Shared.Database.Repository.IPackageRepository>();

            Assert.AreEqual(0, repo.GetCount(1));
        }

        [Test]
        public void MultiplePrereleasePackagesDeleted()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);
            mock.SetupProperty(fc => fc.FeedId, 1);
            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.SetupProperty(fc => fc.IsAbsoluteLatestVersion, false);
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0-alpha"));
            mock.SetupProperty(fc => fc.Version, "1.0.0-alpha");
            mock.SetupProperty(fc => fc.IsPrerelease, true);

            Mock<IInternalPackage> mock2 = new Mock<IInternalPackage>();

            mock2.SetupProperty(fc => fc.Title, "Test Package");
            mock2.SetupProperty(fc => fc.PrimaryId, 1);
            mock2.SetupProperty(fc => fc.FeedId, 1);
            mock2.SetupProperty(fc => fc.Id, "TestPackage");
            mock2.SetupProperty(fc => fc.IsAbsoluteLatestVersion, false);
            mock2.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.1-alpha"));
            mock2.SetupProperty(fc => fc.Version, "1.0.1-alpha");
            mock2.SetupProperty(fc => fc.IsPrerelease, true);

            Mock<IInternalPackage> mock3 = new Mock<IInternalPackage>();

            mock3.SetupProperty(fc => fc.Title, "Test Package");
            mock3.SetupProperty(fc => fc.PrimaryId, 1);
            mock3.SetupProperty(fc => fc.IsAbsoluteLatestVersion, true);
            mock3.SetupProperty(fc => fc.FeedId, 1);
            mock3.SetupProperty(fc => fc.Id, "TestPackage");
            mock3.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.2-alpha"));
            mock3.SetupProperty(fc => fc.Version, "1.0.2-alpha");
            mock3.SetupProperty(fc => fc.IsPrerelease, true);

            var builder = new ContainerBuilder();
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterModule(new TestableConfigurationModule());
            builder.RegisterModule(new PortalModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new TestableSchedulerModule());
            builder.RegisterModule(new NuGetModule());
            builder.RegisterModule(
                new TestableDatabaseModule()
                    .WithFeeds(new List<Feed>
                    {
                        new Feed {Id = 1, Name = "Test", GroupId = 1}
                    })
                    .WithFeedConfigurations(new List<FeedConfiguration>
                    {
                        new FeedConfiguration {FeedId = 1, Directory = "", RetentionPolicyEnabled = true, MaxPrereleasePackages = 2}
                    })
                    .WithPackages(new List<IInternalPackage>
                    {
                        mock.Object, mock2.Object, mock3.Object
                    }));

            _container = builder.Build();

            TestRunPackageRetentionPoliciesJob task = _container.Resolve<TestRunPackageRetentionPoliciesJob>();
            task.Execute(new JobCancellationToken(false));

            var repo = _container.Resolve<Shared.Database.Repository.IPackageRepository>();

            Assert.AreEqual(2, repo.GetCount(mock.Object.FeedId));
            Assert.IsFalse(repo.GetAllPackagesForFeed(mock.Object.FeedId).Contains(mock.Object));
            Assert.Contains(mock2.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock2.Object.FeedId).ToList()));
            Assert.Contains(mock3.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock3.Object.FeedId).ToList()));
        }

        [Test]
        public void MultipleReleasePackagesDeleted()
        {
            Mock<IInternalPackage> mock = new Mock<IInternalPackage>();

            mock.SetupProperty(fc => fc.Title, "Test Package");
            mock.SetupProperty(fc => fc.PrimaryId, 1);
            mock.SetupProperty(fc => fc.FeedId, 1);
            mock.SetupProperty(fc => fc.Id, "TestPackage");
            mock.SetupProperty(fc => fc.IsAbsoluteLatestVersion, false);
            mock.SetupProperty(fc => fc.IsLatestVersion, false);
            mock.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.0-alpha"));
            mock.SetupProperty(fc => fc.Version, "1.0.0");
            mock.SetupProperty(fc => fc.IsPrerelease, false);

            Mock<IInternalPackage> mock2 = new Mock<IInternalPackage>();

            mock2.SetupProperty(fc => fc.Title, "Test Package");
            mock2.SetupProperty(fc => fc.PrimaryId, 1);
            mock2.SetupProperty(fc => fc.FeedId, 1);
            mock2.SetupProperty(fc => fc.Id, "TestPackage");
            mock2.SetupProperty(fc => fc.IsAbsoluteLatestVersion, true);
            mock2.SetupProperty(fc => fc.IsLatestVersion, true);
            mock2.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.1-alpha"));
            mock2.SetupProperty(fc => fc.Version, "1.0.1");
            mock2.SetupProperty(fc => fc.IsPrerelease, false);

            var builder = new ContainerBuilder();
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterModule(new TestableConfigurationModule());
            builder.RegisterModule(new PortalModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new TestableSchedulerModule());
            builder.RegisterModule(new NuGetModule());
            builder.RegisterModule(
                new TestableDatabaseModule()
                    .WithFeeds(new List<Feed>
                    {
                        new Feed {Id = 1, Name = "Test", GroupId = 1}
                    })
                    .WithFeedConfigurations(new List<FeedConfiguration>
                    {
                        new FeedConfiguration {FeedId = 1, Directory = "", RetentionPolicyEnabled = true, MaxPrereleasePackages = 0, MaxReleasePackages = 1}
                    })
                    .WithPackages(new List<IInternalPackage>
                    {
                        mock.Object, mock2.Object
                    }));

            _container = builder.Build();

            TestRunPackageRetentionPoliciesJob task = _container.Resolve<TestRunPackageRetentionPoliciesJob>();
            task.Execute(new JobCancellationToken(false));

            var repo = _container.Resolve<Shared.Database.Repository.IPackageRepository>();

            Assert.AreEqual(1, repo.GetCount(mock.Object.FeedId));
            Assert.IsFalse(repo.GetAllPackagesForFeed(mock.Object.FeedId).Contains(mock.Object));
            Assert.Contains(mock2.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock2.Object.FeedId).ToList()));
        }

        [Test]
        public void SingleReleasePackageNotDeleted()
        {
            Mock<IInternalPackage> mock2 = new Mock<IInternalPackage>();

            mock2.SetupProperty(fc => fc.Title, "Test Package");
            mock2.SetupProperty(fc => fc.PrimaryId, 1);
            mock2.SetupProperty(fc => fc.FeedId, 1);
            mock2.SetupProperty(fc => fc.Id, "TestPackage");
            mock2.SetupProperty(fc => fc.IsAbsoluteLatestVersion, true);
            mock2.SetupProperty(fc => fc.IsLatestVersion, true);
            mock2.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.1-alpha"));
            mock2.SetupProperty(fc => fc.Version, "1.0.1");
            mock2.SetupProperty(fc => fc.IsPrerelease, false);

            var builder = new ContainerBuilder();
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterModule(new TestableConfigurationModule());
            builder.RegisterModule(new PortalModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new TestableSchedulerModule());
            builder.RegisterModule(new NuGetModule());
            builder.RegisterModule(
                new TestableDatabaseModule()
                    .WithFeeds(new List<Feed>
                    {
                        new Feed {Id = 1, Name = "Test", GroupId = 1}
                    })
                    .WithFeedConfigurations(new List<FeedConfiguration>
                    {
                        new FeedConfiguration {FeedId = 1, Directory = "", RetentionPolicyEnabled = true, MaxPrereleasePackages = 0, MaxReleasePackages = 1}
                    })
                    .WithPackages(new List<IInternalPackage>
                    {
                     mock2.Object
                    }));

            _container = builder.Build();

            TestRunPackageRetentionPoliciesJob task = _container.Resolve<TestRunPackageRetentionPoliciesJob>();
            task.Execute(new JobCancellationToken(false));

            var repo = _container.Resolve<Shared.Database.Repository.IPackageRepository>();

            Assert.AreEqual(1, repo.GetCount(mock2.Object.FeedId));
            Assert.Contains(mock2.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock2.Object.FeedId).ToList()));
        }

        [Test]
        public void SinglePrereleasePackageNotDeleted()
        {
            Mock<IInternalPackage> mock3 = new Mock<IInternalPackage>();

            mock3.SetupProperty(fc => fc.Title, "Test Package");
            mock3.SetupProperty(fc => fc.PrimaryId, 1);
            mock3.SetupProperty(fc => fc.IsAbsoluteLatestVersion, true);
            mock3.SetupProperty(fc => fc.FeedId, 1);
            mock3.SetupProperty(fc => fc.Id, "TestPackage");
            mock3.Setup(fc => fc.GetSemanticVersion()).Returns(SemanticVersion.Parse("1.0.2-alpha"));
            mock3.SetupProperty(fc => fc.Version, "1.0.2-alpha");
            mock3.SetupProperty(fc => fc.IsPrerelease, true);

            var builder = new ContainerBuilder();
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterModule(new TestableConfigurationModule());
            builder.RegisterModule(new PortalModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new TestableSchedulerModule());
            builder.RegisterModule(new NuGetModule());
            builder.RegisterModule(
                new TestableDatabaseModule()
                    .WithFeeds(new List<Feed>
                    {
                        new Feed {Id = 1, Name = "Test", GroupId = 1}
                    })
                    .WithFeedConfigurations(new List<FeedConfiguration>
                    {
                        new FeedConfiguration {FeedId = 1, Directory = "", RetentionPolicyEnabled = true, MaxPrereleasePackages = 1}
                    })
                    .WithPackages(new List<IInternalPackage>
                    {
                        mock3.Object
                    }));

            _container = builder.Build();

            TestRunPackageRetentionPoliciesJob task = _container.Resolve<TestRunPackageRetentionPoliciesJob>();
            task.Execute(new JobCancellationToken(false));

            var repo = _container.Resolve<Shared.Database.Repository.IPackageRepository>();

            Assert.AreEqual(1, repo.GetCount(mock3.Object.FeedId));
            Assert.Contains(mock3.Object, new Collection<IInternalPackage>(repo.GetAllPackagesForFeed(mock3.Object.FeedId).ToList()));
        }
    }
}