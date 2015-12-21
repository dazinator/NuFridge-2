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
        protected virtual void LoadLocalRepository(ContainerBuilder builder)
        {
            builder.RegisterType<InternalPackageIndex>().AsSelf().InstancePerDependency();
            builder.RegisterType<InternalPackageRepository>().As<IInternalPackageRepository>().InstancePerDependency();

            builder.Register<Func<int, InternalPackageIndex>>(c => feedId =>
                new InternalPackageIndex(c.Resolve<IPackageService>(), c.Resolve<IPackageDownloadService>(), feedId)
                ).InstancePerDependency();
        }

        protected virtual void LoadRemoteRepository(ContainerBuilder builder)
        {
            builder.RegisterType<RemoteRemotePackageImporter>().As<IRemotePackageImporter>().AsSelf();
            builder.RegisterType<RemoteRemotePackageRepository>().As<IRemotePackageRepository>().AsSelf();
        }

        protected virtual void LoadSourceSymbols(ContainerBuilder builder)
        {
            builder.RegisterType<SymbolSource>().AsSelf().SingleInstance();
            builder.Register(c => new SymbolTools(c.Resolve<IHomeConfiguration>().WindowsDebuggingToolsPath)).AsSelf();
        }

        protected virtual void LoadFileSystems(ContainerBuilder builder)
        {
            builder.Register<Func<int, IPackagePathResolver>>(c =>
                feedId => new NuGetPackagePathResolver(c.Resolve<IFeedConfigurationService>(), feedId)
                ).InstancePerDependency();

            builder.Register<Func<int, IFileSystem>>(c =>
                feedId => new NuGetFileSystem(c.Resolve<IFeedConfigurationService>(), feedId)
                ).InstancePerDependency();
        }

        protected virtual void LoadFactory(ContainerBuilder builder)
        {
            builder.Register<Func<int, IInternalPackageRepositoryFactory>>(
                c =>
                    (feedId =>
                        new InternalPackageRepositoryFactory(i => new InternalPackageRepository(
                            c.Resolve<Func<int, InternalPackageIndex>>(),
                            c.Resolve<Func<int, IPackagePathResolver>>(),
                            c.Resolve<Func<int, IFileSystem>>(), c.Resolve<SymbolSource>(),
                            c.Resolve<IFrameworkNamesManager>(), c.Resolve<IFeedConfigurationService>(), i))))
                .InstancePerDependency();

            builder.RegisterType<InternalPackageRepositoryFactory>().As<IInternalPackageRepositoryFactory>().SingleInstance();
        }



        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            LoadLocalRepository(builder);
            LoadRemoteRepository(builder);
            LoadFactory(builder);
            LoadSourceSymbols(builder);
            LoadFileSystems(builder);
        }
    }
}