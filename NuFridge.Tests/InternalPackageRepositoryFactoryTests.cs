using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.Modules;
using NuFridge.Shared.Server.NuGet;
using NUnit.Framework;

namespace NuFridge.Tests
{
    [TestFixture]
    public class InternalPackageRepositoryFactoryTests
    {
        private IContainer _container;

        [Test]
        public void IsSingleInstanceFactory()
        {
            var factory1 = _container.Resolve<IInternalPackageRepositoryFactory>();
            var factory2 = _container.Resolve<IInternalPackageRepositoryFactory>();

            Assert.AreSame(factory1, factory2);
        }

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new Mock<IHomeConfiguration>().Object);
            builder.RegisterModule(new NuGetModule());
            _container = builder.Build();
        }
    }
}
