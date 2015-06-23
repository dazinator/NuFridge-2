using Autofac;
using NuFridge.Shared.Server.FileSystem;

namespace NuFridge.Shared.Server.Modules
{
  public class FileSystemModule : Module
  {
      protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      builder.RegisterType<PhysicalFileSystem>().As<ILocalFileSystem>();
    }
  }
}
