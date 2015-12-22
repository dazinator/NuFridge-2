using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database;

namespace NuFridge.Tests.Database
{
    public class TestStoreFactory : IStoreFactory
    {
        private readonly Lazy<IStore> _store;

        public IStore Store => _store.Value;

        public TestStoreFactory()
        {
            _store = new Lazy<IStore>(InitializeStore);
        }

        private IStore InitializeStore()
        {
            return new TestStore();
        }
    }
}
