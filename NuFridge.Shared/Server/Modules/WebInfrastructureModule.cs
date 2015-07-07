using Autofac;
using NuFridge.Shared.Server.Web;

namespace NuFridge.Shared.Server.Modules
{
  public class WebInfrastructureModule : Module
  {
      protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      builder.RegisterType<ErrorStatusCodeHandler>().AsSelf();
    }
  }
}
