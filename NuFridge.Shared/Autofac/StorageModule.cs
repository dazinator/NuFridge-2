using Autofac;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Initializers;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet;

namespace NuFridge.Shared.Autofac
{
    public class StorageModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register((c => new StoreFactory(c.Resolve<IHomeConfiguration>()))).As<IStoreFactory>().SingleInstance();
            builder.RegisterType<DatabaseMigrator>().As<IDatabaseMigrator>().SingleInstance();
            builder.RegisterType<DefaultDataInitalizer>().AsSelf();
            builder.RegisterType<MigrationInitializer>().AsSelf();
            builder.RegisterType<PackageHashAndSizeInitalizer>().AsSelf();

            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<UserRepository>().As<IUserRepository>();

            builder.RegisterType<FeedService>().As<IFeedService>();
            builder.RegisterType<FeedRepository>().As<IFeedRepository>();

            builder.RegisterType<FeedGroupService>().As<IFeedGroupService>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();

            builder.RegisterType<PackageService>().As<IPackageService>();
            builder.RegisterType<PackageRepository>().As<IPackageRepository>();

            builder.RegisterType<FrameworkService>().As<IFrameworkService>();
            builder.RegisterType<FrameworkRepository>().As<IFrameworkRepository>();

            builder.RegisterType<FeedGroupService>().As<IFeedGroupService>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();

            builder.RegisterType<PackageDownloadService>().As<IPackageDownloadService>().AsSelf();
            builder.RegisterType<PackageDownloadRepository>().As<IPackageDownloadRepository>();

            builder.RegisterType<FeedManager>().As<IFeedManager>();

            builder.RegisterType<FeedConfigurationService>().As<IFeedConfigurationService>();
            builder.RegisterType<FeedConfigurationRepository>().As<IFeedConfigurationRepository>();

            builder.Register((c => new StoreInitializer(c.Resolve<IStore>(), new IInitializeStore[]
            {
                c.Resolve<MigrationInitializer>(),
                c.Resolve<DefaultDataInitalizer>(),
                c.Resolve<PackageHashAndSizeInitalizer>()
            }))).As<IStoreInitializer>();

            builder.Register((c => c.Resolve<IStoreFactory>().Store)).As<IStore>().SingleInstance();
        }
    }
}