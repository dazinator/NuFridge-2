using Autofac;
using NuFridge.Shared.FileSystem;

namespace NuFridge.Shared.Autofac
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
