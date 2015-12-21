using Autofac;
using NuFridge.Shared.Application;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Web;
using NuFridge.Shared.Web.Listeners;

namespace NuFridge.Shared.Autofac
{
    public class PortalModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            LoadPageListeners(builder);
            LoadConfiguration(builder);
        }

        protected virtual void LoadConfiguration(ContainerBuilder builder)
        {
            builder.RegisterType<WebPortalConfiguration>().As<IWebPortalConfiguration>().SingleInstance();
            builder.RegisterType<WebBootstrapper>().As<IPortalBootstrapper>().SingleInstance();
            builder.RegisterType<WebServerInitializer>().As<IWebServerInitializer>().SingleInstance();
        }

        protected virtual void LoadPageListeners(ContainerBuilder builder)
        {
            builder.RegisterType<StartupPageListener>().As<IStartupPageListener>();
            builder.RegisterType<ShutdownPageListener>().As<IShutdownPageListener>();
        }
    }
}