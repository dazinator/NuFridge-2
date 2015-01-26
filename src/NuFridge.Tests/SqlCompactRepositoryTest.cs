//using System;
//using System.Collections.Generic;
//using System.Data.Services.Common;
//using System.IO;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;
//using NuFridge.Common.Manager;
//using NUnit.Framework;
//using NuGet;
//using NuFridge.DataAccess.Repositories;
//using NuFridge.DataAccess.Model;
//using System.Data.SqlServerCe;

//namespace NuFridge.Tests
//{

//    [TestFixture]
//    public class SqlCompactRepositoryTests
//    {

//        private SqlCompactRepository<Feed> _sut = new SqlCompactRepository<Feed>();

//        [TestFixtureSetUp]
//        public void SetUp()
//        {
//            // Delete any existing sql compact database.
//            var filePath = Environment.CurrentDirectory;
//            var dbPath = System.IO.Path.Combine(filePath, "NuFridge.DataAccess.Repositories.NuFridgeContext.sdf");
//            System.IO.File.Delete(dbPath);            
//        }

//        [Test]
//        public void Can_Insert_Entity()
//        {
//            var feed = new Feed();
//            feed.APIKey = Guid.NewGuid().ToString();
//            feed.FeedURL = "http://someurl.com/odata";
//            feed.Name = "unit test " + DateTime.Now.Ticks;
//            var result = _sut.Insert(feed);
//        }

//        [Test]
//        public void Can_Get_All_Entities()
//        {
//            var results = _sut.GetAll();
//        }       


//    }

//}
