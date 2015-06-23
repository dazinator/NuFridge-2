using Autofac;
using NuFridge.Shared.Server.Web;

namespace NuFridge.Shared.Server.Modules
{
    public class WebModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<WebBootstrapper>().As<IPortalBootstrapper>().SingleInstance();
            builder.RegisterModule(new WebInfrastructureModule());
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterType<WebServerInitializer>().As<IWebServerInitializer>().SingleInstance();
        }
    }
}