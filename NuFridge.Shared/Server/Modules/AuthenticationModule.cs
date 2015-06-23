using Autofac;
using Nancy.Authentication.Token;

namespace NuFridge.Shared.Server.Modules
{
  public class AuthenticationModule : Module
  {
      protected override void Load(ContainerBuilder builder)
    {
      base.Load(builder);
      builder.RegisterType<Tokenizer>().As<ITokenizer>();
    }
  }
}
