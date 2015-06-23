using System;
using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Server.Storage
{
    public class StoreFactory : IStoreFactory
    {
        private readonly string _connectionString;
        private readonly Lazy<Store> _relationalStore;
        private readonly IHomeConfiguration _config;

        public Store Store
        {
            get
            {
                return _relationalStore.Value;
            }
        }

        public StoreFactory(IHomeConfiguration config, string connectionString)
        {
            _config = config;
            _connectionString = connectionString;
            _relationalStore = new Lazy<Store>(InitializeRelationalStore);
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
            return new Store(_config, _connectionString, CreateMappings());
        }
    }
}
