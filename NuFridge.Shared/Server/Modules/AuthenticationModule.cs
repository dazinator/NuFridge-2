using System;
using Autofac;
using Nancy.Authentication.Token;
using Nancy.Authentication.Token.Storage;

namespace NuFridge.Shared.Server.Modules
{
    public class AuthenticationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.Register<ITokenizer>(context => new Tokenizer(ConfigureTokenzier));
        }

        private void ConfigureTokenzier(Tokenizer.TokenizerConfigurator tokenizerConfigurator)
        {
            tokenizerConfigurator.KeyExpiration(SetKeyExpiration);
            tokenizerConfigurator.TokenExpiration(SetTokenExpiration);
        }

        private TimeSpan SetTokenExpiration()
        {
            return TimeSpan.FromDays(7);
        }

        private TimeSpan SetKeyExpiration()
        {
            return TimeSpan.FromDays(14);
        }
    }
}