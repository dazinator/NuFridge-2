using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Model.Mappings;
using NuFridge.Shared.Server.Storage;
using NUnit.Framework;

namespace NuFridge.Tests
{
    [TestFixture]
    public class ModelMappingTests
    {
        [Test]
        [TestCase(typeof(IFeed))]
        [TestCase(typeof(IFeedConfiguration))]
        [TestCase(typeof(ApiKey))]
        [TestCase(typeof(IInternalPackage))]
        [TestCase(typeof(IStatistic))]
        [TestCase(typeof(User))]
        [TestCase(typeof(UserRole))]
        public void HasMappingConfiguration(Type type)
        {
            var mappings = StoreFactory.CreateMappings();


            EntityMapping mapping;
            var success = mappings.TryGet(type, out mapping);

            Assert.IsTrue(success, "No mapping configuration has been setup for type " + type.Name + ".");
            Assert.NotNull(mapping);

            Assert.IsNotNullOrEmpty(mapping.TableName);
            Assert.That(mapping.IndexedColumns.Count > 0, "No indexed columns for type of " + type.Name + ".");
        }
    }
}