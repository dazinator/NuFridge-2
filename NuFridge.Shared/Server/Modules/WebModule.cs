using Autofac;
using NuFridge.Shared.Server.Web;
using NuFridge.Shared.Server.Web.Actions;

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

            builder.RegisterType<SignInAction>().AsSelf();
            builder.RegisterType<GetFeedsAction>().AsSelf();
            builder.RegisterType<GetFeedAction>().AsSelf();
        }
    }
}