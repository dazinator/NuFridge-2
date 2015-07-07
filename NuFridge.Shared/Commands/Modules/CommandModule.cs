using Autofac;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Commands.Util;
using NuFridge.Shared.Extensions;

namespace NuFridge.Shared.Commands.Modules
{
  public class CommandModule : Module
  {
      protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      builder.RegisterCommand<RunCommand>("run", "Starts the NuFridge server", "r", "");
      builder.RegisterCommand<HelpCommand>("help", "Prints this help text", "h", "?");
      builder.RegisterCommand<ConfigureCommand>("configure", "Configures the NuFridge server", "c");
      builder.RegisterType<CommandLocator>().As<ICommandLocator>().SingleInstance();
    }
  }
}
