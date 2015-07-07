using Autofac;
using NuFridge.Shared.Commands;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Server;
using NuFridge.Shared.Server.Application;
using NuFridge.Shared.Server.Modules;

namespace NuFridge.Service
{
    public class Startup : StartupBase
    {
        public Startup(string[] commandLineArguments)
            : base("NuFridge Server", commandLineArguments)
        {
        }

        public  static int Run(string[] args)
        {
            return new Startup(args).Run();
        }

        protected override IContainer BuildContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new FullModule());
            return builder.Build();
        }
    }
}
