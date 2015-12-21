using Autofac;
using NuFridge.Shared.Application;
using NuFridge.Shared.Autofac;
using NuFridge.Tests.Application;

namespace NuFridge.Tests.Autofac
{
    public class TestableConfigurationModule : ConfigurationModule
    {
        protected override void LoadInstanceStore(ContainerBuilder builder)
        {
            builder.RegisterType<TestApplicationInstanceStore>().As<IApplicationInstanceStore>();
        }

        protected override void LoadHomeConfiguration(ContainerBuilder builder)
        {
            builder.Register(c => new TestHomeConfiguration()).As<IHomeConfiguration>().SingleInstance();
        }
    }
}