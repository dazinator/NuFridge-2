using Autofac;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Autofac
{
  public class ConfigurationModule : Module
  {
      protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      builder.RegisterType<ApplicationInstanceStore>().As<IApplicationInstanceStore>();
      builder.Register( (c => new ApplicationInstanceSelector(c.Resolve<IApplicationInstanceStore>()))).As<IApplicationInstanceSelector>().SingleInstance();
      builder.Register((c => new HomeConfiguration(c.Resolve<IApplicationInstanceSelector>()))).As<IHomeConfiguration>().SingleInstance();
    }
  }
}
