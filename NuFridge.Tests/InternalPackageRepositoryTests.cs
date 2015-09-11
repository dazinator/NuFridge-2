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
using NuFridge.Shared.NuGet.Repository;
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

        private void MockStore(ContainerBuilder builder) 
        {
            Mock<IStore> store = new Mock<IStore>();
            builder.RegisterInstance(store.Object);

            //Mock<ITransaction> transaction = new Mock<ITransaction>();
            //store.Setup(st => st.BeginTransaction()).Returns(transaction.Object);

            //Mock<IQueryBuilder<IFeedConfiguration>> queryMock = new Mock<IQueryBuilder<IFeedConfiguration>>();
            //transaction.Setup(tr => tr.Query<IFeedConfiguration>()).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.Where(It.IsAny<string>())).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.Parameter(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.LikeParameter(It.IsAny<string>(), It.IsAny<object>())).Returns(queryMock.Object);
            //queryMock.Setup(qu => qu.OrderBy(It.IsAny<string>())).Returns(queryMock.Object);

            //Mock<IFeedConfiguration> mock = new Mock<IFeedConfiguration>();

            //mock.SetupProperty(fc => fc.Directory, "TestDirectory");
            //mock.SetupProperty(fc => fc.Id, 1);
            //mock.SetupProperty(fc => fc.FeedId, 1);

            //queryMock.Setup(qu => qu.ToList()).Returns(new List<IFeedConfiguration> {mock.Object});
            //queryMock.Setup(qu => qu.First()).Returns(mock.Object);
            //queryMock.Setup(qu => qu.Count()).Returns(1);
        }

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new NuGetModule());
            MockStore(builder);
            builder.RegisterInstance(new Mock<IHomeConfiguration>().Object);
            _container = builder.Build();
        }
    }
}