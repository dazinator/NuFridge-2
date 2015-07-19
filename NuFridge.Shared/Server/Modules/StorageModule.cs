using Autofac;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Storage.Initializers;

namespace NuFridge.Shared.Server.Modules
{
    public class StorageModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);


            builder.Register((c => new StoreFactory(c.Resolve<IContainer>(), c.Resolve<IHomeConfiguration>(), c.Resolve<IServerStorageConfiguration>().ExternalDatabaseConnectionString))).As<IStoreFactory>().SingleInstance();
            builder.RegisterType<DatabaseMigrator>().As<IDatabaseMigrator>().SingleInstance();
            builder.RegisterType<AdminUserInitializer>().AsSelf();
            builder.RegisterType<MigrationInitializer>().AsSelf();

            builder.Register((c => new StoreInitializer(c.Resolve<IStore>(), new IInitializeRelationalStore[]
      {
          c.Resolve<MigrationInitializer>(),
          c.Resolve<AdminUserInitializer>()
      }))).As<IStoreInitializer>();

            builder.Register((c => c.Resolve<IStoreFactory>().Store)).As<IStore>().SingleInstance();
            builder.Register((c => c.Resolve<IStore>().BeginTransaction())).As<ITransaction>().InstancePerLifetimeScope();
        }
    }
}
