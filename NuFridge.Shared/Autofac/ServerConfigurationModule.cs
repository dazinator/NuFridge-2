using Autofac;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Autofac
{
    public class ServerConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<WebPortalConfiguration>().As<IWebPortalConfiguration>().SingleInstance();
        }
    }
}