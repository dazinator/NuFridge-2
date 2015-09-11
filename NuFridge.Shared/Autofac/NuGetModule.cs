using System;
using Autofac;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.FileSystem;
using NuFridge.Shared.NuGet;
using NuFridge.Shared.NuGet.Repository;
using NuFridge.Shared.NuGet.Symbols;
using NuGet;

namespace NuFridge.Shared.Autofac
{
    public class NuGetModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<PackageImportJobItemService>().As<IPackageImportJobItemService>();
            builder.RegisterType<JobRepository>().As<IJobRepository>();
            builder.RegisterType<JobService>().As<IJobService>();
            builder.RegisterType<PackageImportJobItemRepository>().As<IPackageImportJobItemRepository>();

            builder.RegisterType<InternalPackageIndex>().AsSelf().InstancePerDependency();
            builder.RegisterType<Feed>().As<IFeed>();
            builder.RegisterType<FeedConfiguration>().As<IFeedConfiguration>();
            builder.RegisterType<InternalPackage>().As<IInternalPackage>();
            builder.RegisterType<InternalPackageRepository>().As<IInternalPackageRepository>().InstancePerDependency();
            builder.RegisterType<Statistic>().As<IStatistic>();
            builder.RegisterType<Framework>().As<IFramework>();
            builder.RegisterType<RemoteRemotePackageImporter>().As<IRemotePackageImporter>().AsSelf();
            builder.RegisterType<RemoteRemotePackageRepository>().As<IRemotePackageRepository>().AsSelf();
            builder.Register<Func<int, InternalPackageIndex>>(c => feedId =>
                new InternalPackageIndex(c.Resolve<IPackageService>(), c.Resolve<IPackageDownloadService>(), feedId)
                ).InstancePerDependency();

            builder.Register<Func<int, IPackagePathResolver>>(c =>
                feedId => new NuGetPackagePathResolver(c.Resolve<IFeedConfigurationService>(), feedId)
                ).InstancePerDependency();

            builder.Register<Func<int, IFileSystem>>(c =>
                feedId => new NuGetFileSystem(c.Resolve<IFeedConfigurationService>(), feedId)
                ).InstancePerDependency();

            builder.RegisterType<FrameworkNamesManager>().As<IFrameworkNamesManager>().SingleInstance();

            builder.RegisterType<SymbolSource>().AsSelf().SingleInstance();
            builder.Register(c => new SymbolTools(c.Resolve<IHomeConfiguration>().WindowsDebuggingToolsPath)).AsSelf();

            builder.RegisterType<InternalPackageRepositoryFactory>().As<IInternalPackageRepositoryFactory>().SingleInstance();

            builder.RegisterType<PackageImportJobRepository>().As<IJobTypeRepository<IJobType>>();

            builder.Register<Func<int, IInternalPackageRepositoryFactory>>(
                c =>
                    (feedId =>
                        new InternalPackageRepositoryFactory(i => new InternalPackageRepository(
                    c.Resolve<Func<int, InternalPackageIndex>>(),
                    c.Resolve<Func<int, IPackagePathResolver>>(),
                    c.Resolve<Func<int, IFileSystem>>(), c.Resolve<SymbolSource>(),
                    c.Resolve<IFrameworkNamesManager>(), c.Resolve<IFeedConfigurationService>(), i)))).InstancePerDependency();
        }
    }
}