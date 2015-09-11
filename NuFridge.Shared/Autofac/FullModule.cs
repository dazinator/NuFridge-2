using Autofac;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Autofac
{
    public class FullModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new ConfigurationModule());
            builder.RegisterModule(new ServerConfigurationModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new StorageModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new NuGetModule());
        }
    }
}