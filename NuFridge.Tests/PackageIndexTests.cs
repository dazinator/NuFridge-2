using System.IO;
using Moq;
using NuFridge.Shared.Model;
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

        [SetUp]
        public void CreateIndexer()
        {
            Store = new Mock<IStore>();
            Index = new TestPackageIndex(Store.Object, 1);
        }

        [Test]
        public void AddPackage()
        {
            var package = TestPackageIndex.GetTestPackage("Test", "1.0.0");

            Mock<ITransaction> transaction = new Mock<ITransaction>();
            Store.Setup(st => st.BeginTransaction()).Returns(transaction.Object);

            Index.AddPackage(package, false, false);

            transaction.Verify(tr => tr.Insert(It.IsAny<IPackage>()), Times.Exactly(1));
            transaction.Verify(tr => tr.Commit(), Times.Exactly(1));
        }

        [Test]
        public void DeletePackage()
        {
            var package = TestPackageIndex.GetTestPackage("Test", "1.0.0");

            Mock<ITransaction> transaction = new Mock<ITransaction>();
            Store.Setup(st => st.BeginTransaction()).Returns(transaction.Object);

            Index.DeletePackage(package);

            transaction.Verify(tr => tr.Delete(It.IsAny<IPackage>()), Times.Exactly(1));
            transaction.Verify(tr => tr.Commit(), Times.Exactly(1));
        }



        public class TestPackageIndex : PackageIndex
        {
            public TestPackageIndex(IStore store, int feedId) : base(store, feedId)
            {
                
            }

            public static InternalPackage GetTestPackage(string id, string version)
            {
                var p = new InternalPackage
                {
                    Id = 1,
                    PackageId = id,
                    Version = version != null ? new SemanticVersion(version) : null,
                    DownloadCount = -1,
                    VersionDownloadCount = -1
                };

                return p;
            }

            protected override InternalPackage LoadPackage(string id, string version)
            {
                return GetTestPackage(id, version);
            }
        }
    }
}