using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet;

namespace NuFridge.Shared.Server.Storage.Initializers
{
    public class InternalPackageRepositoryFactoryInitializer : IInitializeStore
    {
        private readonly ILog _log = LogProvider.For<EntityFrameworkInitializer>();

        private readonly IInternalPackageRepositoryFactory _factory;

        public InternalPackageRepositoryFactoryInitializer(IInternalPackageRepositoryFactory factory)
        {
            _factory = factory;
        }

        public void Initialize(IStore store)
        {
            _log.Debug("Initializing the internal package repository factory.");

            _factory.Init();
        }
    }
}