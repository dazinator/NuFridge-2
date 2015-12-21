using Autofac;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Initializers;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet;

namespace NuFridge.Shared.Autofac
{
    public class DatabaseModule : Module
    {
        protected virtual void LoadRepositorys(ContainerBuilder builder)
        {
            builder.RegisterType<PackageDownloadRepository>().As<IPackageDownloadRepository>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();
            builder.RegisterType<PackageRepository>().As<IPackageRepository>();
            builder.RegisterType<FeedGroupRepository>().As<IFeedGroupRepository>();
            builder.RegisterType<FeedRepository>().As<IFeedRepository>();
            builder.RegisterType<UserRepository>().As<IUserRepository>();
            builder.RegisterType<JobRepository>().As<IJobRepository>();
            builder.RegisterType<FrameworkRepository>().As<IFrameworkRepository>();
            builder.RegisterType<PackageDownloadRepository>().As<IPackageDownloadRepository>();
            builder.RegisterType<FeedConfigurationRepository>().As<IFeedConfigurationRepository>();
            builder.RegisterType<PackageImportJobItemRepository>().As<IPackageImportJobItemRepository>();
            builder.RegisterType<PackageImportJobRepository>().As<IJobTypeRepository<IJobType>>();
        }

        protected virtual void LoadServices(ContainerBuilder builder)
        {
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<FeedService>().As<IFeedService>();
            builder.RegisterType<FeedGroupService>().As<IFeedGroupService>();
            builder.RegisterType<PackageService>().As<IPackageService>();
            builder.RegisterType<JobService>().As<IJobService>();
            builder.RegisterType<FrameworkService>().As<IFrameworkService>();
            builder.RegisterType<FeedConfigurationService>().As<IFeedConfigurationService>();
            builder.RegisterType<PackageImportJobItemService>().As<IPackageImportJobItemService>();
            builder.RegisterType<FeedGroupService>().As<IFeedGroupService>();
            builder.RegisterType<PackageDownloadService>().As<IPackageDownloadService>().AsSelf();
        }

        protected virtual void LoadManagers(ContainerBuilder builder)
        {
            builder.RegisterType<FeedManager>().As<IFeedManager>();
            LoadFrameworkNameManager(builder);
        }

        protected virtual void LoadFrameworkNameManager(ContainerBuilder builder)
        {
            builder.RegisterType<FrameworkNamesManager>().As<IFrameworkNamesManager>().SingleInstance();
        }

        protected virtual void LoadInitalizers(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseMigrator>().As<IDatabaseMigrator>().SingleInstance();
            builder.RegisterType<DefaultDataInitalizer>().AsSelf();
            builder.RegisterType<MigrationInitializer>().AsSelf();
            builder.RegisterType<PackageHashAndSizeInitalizer>().AsSelf();
            builder.Register(c => new StoreInitializer(c.Resolve<IStore>(), new IInitializeStore[]
            {
                c.Resolve<MigrationInitializer>(),
                c.Resolve<DefaultDataInitalizer>(),
                c.Resolve<PackageHashAndSizeInitalizer>()
            })).As<IStoreInitializer>();
        }

        protected virtual void LoadStore(ContainerBuilder builder)
        {
            builder.RegisterType<DatabaseContext>().AsSelf();
            builder.Register((c => new StoreFactory(c.Resolve<IHomeConfiguration>()))).As<IStoreFactory>().SingleInstance();
            builder.Register((c => c.Resolve<IStoreFactory>().Store)).As<IStore>().SingleInstance();
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            LoadStore(builder);
            LoadRepositorys(builder);
            LoadServices(builder);
            LoadManagers(builder);
            LoadInitalizers(builder);
        }
    }
}