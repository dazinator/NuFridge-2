using System;
using Autofac;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.Import;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuGet;

namespace NuFridge.Shared.Server.Modules
{
    public class NuGetModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<PackageIndex>().AsSelf().InstancePerDependency();
            builder.RegisterType<Feed>().As<IFeed>();
            builder.RegisterType<FeedConfiguration>().As<IFeedConfiguration>();
            builder.RegisterType<InternalPackage>().As<IInternalPackage>();
            builder.RegisterType<InternalPackageRepository>().As<IInternalPackageRepository>().InstancePerDependency();
            builder.RegisterType<Statistic>().As<IStatistic>();
            builder.RegisterType<Framework>().As<IFramework>();
            builder.RegisterType<PackageImporter>().As<IPackageImporter>().AsSelf();
            builder.RegisterType<PackageImportRepository>().As<IPackageImportRepository>().AsSelf();
            builder.Register<Func<int, PackageIndex>>(c => feedId =>
                new PackageIndex(c.Resolve<IPackageService>(), c.Resolve<IPackageDownloadService>(), feedId)
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

            builder.Register<Func<int, IInternalPackageRepositoryFactory>>(
                c =>
                    (feedId =>
                        new InternalPackageRepositoryFactory(i => new InternalPackageRepository(
                    c.Resolve<Func<int, PackageIndex>>(),
                    c.Resolve<Func<int, IPackagePathResolver>>(),
                    c.Resolve<Func<int, IFileSystem>>(), c.Resolve<SymbolSource>(),
                    c.Resolve<IFrameworkNamesManager>(), c.Resolve<IFeedConfigurationService>(), i)))).InstancePerDependency();
        }
    }
}