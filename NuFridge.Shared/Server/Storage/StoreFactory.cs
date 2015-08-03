using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Storage
{
    public class StoreFactory : IStoreFactory
    {
        private readonly Lazy<Store> _store;
        private readonly IHomeConfiguration _config;
        private readonly IContainer _container;

        public Store Store => _store.Value;

        public StoreFactory(IContainer container, IHomeConfiguration config)
        {
            _container = container;
            _config = config;
            _store = new Lazy<Store>(InitializeRelationalStore);
        }

        public static RelationalMappings CreateMappings()
        {
            RelationalMappings relationalMappings = new RelationalMappings();
            List<EntityMapping> list =
                typeof (StoreFactory).Assembly.GetTypes().Where(
                    type => typeof (EntityMapping).IsAssignableFrom(type))
                    .Where(type =>
                    {
                        if (type.IsClass)
                            return !type.IsAbstract && !type.ContainsGenericParameters;
                        return false;
                    })
                    .Select(
                        type => Activator.CreateInstance(type) as EntityMapping)
                    .ToList();
            relationalMappings.Install(list);
            return relationalMappings;
        }

        private Store InitializeRelationalStore()
        {
            return new Store(_container, _config, CreateMappings());
        }
    }
}
