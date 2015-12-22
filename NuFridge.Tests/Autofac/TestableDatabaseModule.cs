using System.Collections.Generic;
using Autofac;
using NuFridge.Shared.Application;
using NuFridge.Shared.Autofac;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.NuGet;
using NuFridge.Tests.Database;
using NuFridge.Tests.Database.Repository;
using NuFridge.Tests.NuGet;

namespace NuFridge.Tests.Autofac
{
    public class TestableDatabaseModule : DatabaseModule
    {
        private readonly TestFeedConfigurationRepository _testFeedConfigurationRepository = new TestFeedConfigurationRepository();
        private readonly TestFeedRepository _testFeedRepository = new TestFeedRepository();
        private readonly TestPackageRepository _testPackageRepository = new TestPackageRepository();

        protected override void LoadFrameworkNameManager(ContainerBuilder builder)
        {
            builder.RegisterType<TestFrameworkNamesManager>().As<IFrameworkNamesManager>().SingleInstance();
        }

        public TestableDatabaseModule WithPackages(List<IInternalPackage> packages)
        {
            _testPackageRepository.WithPackages(packages);

            return this;
        }

        public TestableDatabaseModule WithFeeds(List<Feed> feeds)
        {
            _testFeedRepository.WithFeeds(feeds);

            return this;
        }

        public TestableDatabaseModule WithFeedConfigurations(List<FeedConfiguration> feedConfigurations)
        {
            _testFeedConfigurationRepository.WithFeedConfigurations(feedConfigurations);

            return this;
        }

        protected override void LoadStore(ContainerBuilder builder)
        {
            builder.Register(c => new TestStoreFactory()).As<IStoreFactory>().SingleInstance();
            builder.Register(c => c.Resolve<IStoreFactory>().Store).As<IStore>().SingleInstance();
        }

        protected override void LoadRepositorys(ContainerBuilder builder)
        {
            builder.RegisterInstance(_testFeedConfigurationRepository).As<IFeedConfigurationRepository>();
            builder.RegisterInstance(_testFeedRepository).As<IFeedRepository>();
            builder.RegisterInstance(_testPackageRepository).As<IPackageRepository>();

            builder.RegisterType<PackageDownloadRepository>().As<IPackageDownloadRepository>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();
            builder.RegisterType<UserRepository>().As<IUserRepository>();
            builder.RegisterType<JobRepository>().As<IJobRepository>();
            builder.RegisterType<FrameworkRepository>().As<IFrameworkRepository>();
            builder.RegisterType<PackageDownloadRepository>().As<IPackageDownloadRepository>();
            builder.RegisterType<PackageImportJobItemRepository>().As<IPackageImportJobItemRepository>();
            builder.RegisterType<PackageImportJobRepository>().As<IJobTypeRepository<IJobType>>();
        }
    }
}