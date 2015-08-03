using Autofac;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Diagnostics;
using NuFridge.Shared.Server.Storage;

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