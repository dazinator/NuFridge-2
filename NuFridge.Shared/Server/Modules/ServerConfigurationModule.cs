using Autofac;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Modules
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