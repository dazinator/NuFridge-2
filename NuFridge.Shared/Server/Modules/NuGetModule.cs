using System;
using Autofac;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;


namespace NuFridge.Shared.Server.Modules
{
    public class NuGetModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<PackageIndex>().AsSelf();

            builder.RegisterType<InternalPackageRepository>().As<IInternalPackageRepository>();

            builder.Register<Func<int, PackageIndex>>(c => (feedId => new PackageIndex(c.Resolve<IStore>(), feedId))).InstancePerDependency();
            builder.Register<Func<int, IPackagePathResolver>>(c => (feedId => CreatePathResolver(c, feedId))).InstancePerDependency();
            builder.Register<Func<int, IFileSystem>>(c => (feedId => CreateFileSystem(c, feedId))).InstancePerDependency();

            builder.RegisterType<InternalPackageRepositoryFactory>().AsSelf().SingleInstance();

            builder.Register<Func<int, InternalPackageRepositoryFactory>>(
                c =>
                    (feedId =>
                        new InternalPackageRepositoryFactory(
                            i =>
                                new InternalPackageRepository(
                                    c.Resolve<Func<int, PackageIndex>>(),
                                    c.Resolve<Func<int, IPackagePathResolver>>(),
                                    c.Resolve<Func<int, IFileSystem>>(), i)))).InstancePerDependency();
        }

        //TODO move this elsewhere or rethink on how to do this better
        private IFileSystem CreateFileSystem(IComponentContext c, int feedId)
        {
            var store = c.Resolve<IStore>();

            using (var transaction = store.BeginTransaction())
            {
                var config = transaction.Query<FeedConfiguration>().Where("FeedId = @feedId").Parameter("feedId", feedId).First();

                return new PhysicalFileSystem(config.PackagesDirectory);
            }
        }

        //TODO move this elsewhere or rethink on how to do this better
        private IPackagePathResolver CreatePathResolver(IComponentContext c, int feedId)
        {
            var store = c.Resolve<IStore>();

            using (var transaction = store.BeginTransaction())
            {
                var config = transaction.Query<FeedConfiguration>().Where("FeedId = @feedId").Parameter("feedId", feedId).First();

                return new DefaultPackagePathResolver(config.PackagesDirectory);
            }
        }
    }
}