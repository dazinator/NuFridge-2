using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NuFridge.Shared.Application;
using NuFridge.Shared.Autofac;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.NuGet.Repository;
using NuFridge.Tests.Autofac;
using NUnit.Framework;

namespace NuFridge.Tests
{
    [TestFixture]
    public class InternalPackageRepositoryTests
    {
        private IContainer _container;

        [Test]
        public void IsInstancePerDependency()
        {
            var factory = _container.Resolve<IInternalPackageRepositoryFactory>();

            var packageRepo1 = factory.Create(1);
            var packageRepo2 = factory.Create(2);

            Assert.AreNotSame(packageRepo1, packageRepo2);
            Assert.AreNotEqual(packageRepo1.FeedId, packageRepo2.FeedId);
            Assert.AreEqual(1, packageRepo1.FeedId);
            Assert.AreEqual(2, packageRepo2.FeedId);
        }

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ServerEngine>().As<IServerEngine>().SingleInstance();
            builder.RegisterModule(new AuthenticationModule());
            builder.RegisterModule(new TestableConfigurationModule());
            builder.RegisterModule(new PortalModule());
            builder.RegisterModule(new FileSystemModule());
            builder.RegisterModule(new WebModule());
            builder.RegisterModule(new NuGetModule());
            builder.RegisterModule(new TestableDatabaseModule().WithFeedConfigurations(new List<FeedConfiguration>
            {
                new FeedConfiguration {FeedId = 1, Directory = ""},
                new FeedConfiguration {FeedId = 2, Directory = ""}
            }));
            _container = builder.Build();
        }
    }
}