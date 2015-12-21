using Autofac;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Autofac
{
    public class ConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            LoadInstanceStore(builder);
            LoadHomeConfiguration(builder);

            builder.Register((c => new ApplicationInstanceSelector(c.Resolve<IApplicationInstanceStore>()))).As<IApplicationInstanceSelector>().SingleInstance();
        }

        protected virtual void LoadHomeConfiguration(ContainerBuilder builder)
        {
            builder.Register((c => new HomeConfiguration(c.Resolve<IApplicationInstanceSelector>()))).As<IHomeConfiguration>().SingleInstance();
        }

        protected virtual void LoadInstanceStore(ContainerBuilder builder)
        {
            builder.RegisterType<ApplicationInstanceStore>().As<IApplicationInstanceStore>();
        }
    }
}