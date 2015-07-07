using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NuFridge.Shared.Server.Storage
{
    public class RelationalMappings
    {
        private readonly ConcurrentDictionary<Type, EntityMapping> _mappings = new ConcurrentDictionary<Type, EntityMapping>();

        public List<EntityMapping> GetAll()
        {
            return new List<EntityMapping>(_mappings.Values);
        }

        public void Install(IEnumerable<EntityMapping> mappingsToAdd)
        {
            foreach (EntityMapping documentMap in mappingsToAdd)
                _mappings[documentMap.Type] = documentMap;
        }

        public bool TryGet(Type type, out EntityMapping map)
        {
            return _mappings.TryGetValue(type, out map);
        }

        public EntityMapping Get(Type type)
        {
            EntityMapping documentMap;
            if (!_mappings.TryGetValue(type, out documentMap))
                throw new KeyNotFoundException(string.Format("A mapping for the type '{0}' has not been defined", type.Name));
            return documentMap;
        }
    }
}
