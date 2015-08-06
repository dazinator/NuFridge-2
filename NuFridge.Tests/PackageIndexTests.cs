using System.IO;
using Moq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;
using NUnit.Framework;

namespace NuFridge.Tests
{
    [TestFixture]
    public class PackageIndexTests
    {
        protected TestPackageIndex Index;
        protected Mock<IStore> Store;
        protected Mock<IInternalPackageRepositoryFactory> Factory;

        [SetUp]
        public void CreateIndexer()
        {
            Store = new Mock<IStore>();
            Factory = new Mock<IInternalPackageRepositoryFactory>();
            Index = new TestPackageIndex(1);
        }

        //[Test]
        //public void AddPackage()
        //{
        //    var package = TestPackageIndex.GetTestPackage("Test", "1.0.0");

        //    Mock<ITransaction> transaction = new Mock<ITransaction>();
        //    Store.Setup(st => st.BeginTransaction()).Returns(transaction.Object);

        //    Index.AddPackage(package, false, false);

        //    transaction.Verify(tr => tr.Insert(It.IsAny<IPackage>()), Times.Once);
        //    transaction.Verify(tr => tr.Commit(), Times.Once);
        //    transaction.Verify(tr => tr.Dispose(), Times.Once);
        //}

        [Test]
        public void DeletePackage()
        {
            var package = TestPackageIndex.GetTestPackage("Test", "1.0.0");

            //Mock<ITransaction> transaction = new Mock<ITransaction>();
            //Store.Setup(st => st.BeginTransaction()).Returns(transaction.Object);

            ////Index.DeletePackage(package);

            //transaction.Verify(tr => tr.Delete(It.IsAny<IInternalPackage>()), Times.Once);
            //transaction.Verify(tr => tr.Commit(), Times.Once);
            //transaction.Verify(tr => tr.Dispose(), Times.Once);
        }



        public class TestPackageIndex : PackageIndex
        {
            public TestPackageIndex(int feedId) : base(null, feedId)
            {
                
            }

            public static IInternalPackage GetTestPackage(string id, string version)
            {
                var p = new InternalPackage
                {
                    PrimaryId = 1,
                    Id = id,
                    DownloadCount = -1,
                    VersionDownloadCount = -1
                };

                p.Version = version;

                return p;
            }

            //protected override IInternalPackage LoadPackage(string id, string version)
            //{
            //    return GetTestPackage(id, version) as InternalPackage;
            //}
        }
    }
}