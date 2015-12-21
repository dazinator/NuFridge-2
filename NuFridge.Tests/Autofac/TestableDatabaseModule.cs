using System.Collections.Generic;
using Autofac;
using NuFridge.Shared.Autofac;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.NuGet;
using NuFridge.Tests.Application;
using NuFridge.Tests.Database.Repository;
using NuFridge.Tests.NuGet;

namespace NuFridge.Tests.Autofac
{
    public class TestableDatabaseModule : DatabaseModule
    {
        private readonly TestFeedConfigurationRepository _testFeedConfigurationRepository = new TestFeedConfigurationRepository(new DatabaseContext(new TestHomeConfiguration()));

        protected override void LoadFrameworkNameManager(ContainerBuilder builder)
        {
            builder.RegisterType<TestFrameworkNamesManager>().As<IFrameworkNamesManager>().SingleInstance();
        }

        public TestableDatabaseModule WithFeedConfigurations(List<FeedConfiguration> feedConfigurations)
        {
            _testFeedConfigurationRepository.WithFeedConfigurations(feedConfigurations);

            return this;
        }

        protected override void LoadRepositorys(ContainerBuilder builder)
        {
            builder.RegisterInstance(_testFeedConfigurationRepository).As<IFeedConfigurationRepository>();

            builder.RegisterType<PackageDownloadRepository>().As<IPackageDownloadRepository>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();
            builder.RegisterType<PackageRepository>().As<IPackageRepository>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();
            builder.RegisterType<FeedRepository>().As<IFeedRepository>();
            builder.RegisterType<UserRepository>().As<IUserRepository>();
            builder.RegisterType<JobRepository>().As<IJobRepository>();
            builder.RegisterType<FrameworkRepository>().As<IFrameworkRepository>();
            builder.RegisterType<PackageDownloadRepository>().As<IPackageDownloadRepository>();
            builder.RegisterType<PackageImportJobItemRepository>().As<IPackageImportJobItemRepository>();
            builder.RegisterType<PackageImportJobRepository>().As<IJobTypeRepository<IJobType>>();
        }
    }
}