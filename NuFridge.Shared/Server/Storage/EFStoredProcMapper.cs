using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace NuFridge.Shared.Server.Storage
{
    public class EFStoredProcMapper
    {
        public static IQueryable<T> Map<T>(DbContext context, DbConnection queryConnection, string sqlQuery) where T : new()
        {
            var connectionState = queryConnection.State;

            if (connectionState != ConnectionState.Open)
                queryConnection.Open();

            DbCommand command = queryConnection.CreateCommand();
            command.CommandText = sqlQuery;
            DbDataReader reader = command.ExecuteReader();

            List<T> entities = new List<T>();

            while (reader.Read())
            {
                entities.Add(InternalMap<T>(context, reader));
            }

            if (connectionState != ConnectionState.Open)
                queryConnection.Close();

            return entities.AsQueryable();

        }

        private static T InternalMap<T>(DbContext context, DbDataReader reader) where T : new()
        {

            T entityObject = new T();

            InternalMapEntity(context, reader, entityObject);

            return entityObject;
        }

        private static void InternalMapEntity(DbContext context, DbDataReader reader, object entityObject)
        {

            ObjectContext objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var metadataWorkspace = ((EntityConnection)objectContext.Connection).GetMetadataWorkspace();

            IEnumerable<EntitySetMapping> entitySetMappingCollection = metadataWorkspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().EntitySetMappings;
            IEnumerable<AssociationSetMapping> associationSetMappingCollection = metadataWorkspace.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().AssociationSetMappings;

            var entitySetMappings = entitySetMappingCollection.First(o => o.EntityTypeMappings.Select(e => e.EntityType.Name).Contains(entityObject.GetType().Name));

            var entityTypeMapping = entitySetMappings.EntityTypeMappings[0];
            string tableName = entityTypeMapping.EntitySetMapping.EntitySet.Name;

            MappingFragment mappingFragment = entityTypeMapping.Fragments[0];

            foreach (PropertyMapping propertyMapping in mappingFragment.PropertyMappings)
            {
                object value = Convert.ChangeType(reader[((ScalarPropertyMapping)propertyMapping).Column.Name], propertyMapping.Property.PrimitiveType.ClrEquivalentType);
                entityObject.GetType().GetProperty(propertyMapping.Property.Name).SetValue(entityObject, value, null);
            }

            foreach (var navigationProperty in entityTypeMapping.EntityType.NavigationProperties)
            {
                PropertyInfo propertyInfo = entityObject.GetType().GetProperty(navigationProperty.Name);

                AssociationSetMapping associationSetMapping = associationSetMappingCollection.First(a => a.AssociationSet.ElementType.FullName == navigationProperty.RelationshipType.FullName);

                EndPropertyMapping propertyMappings = associationSetMapping.AssociationTypeMapping.MappingFragment.PropertyMappings.Cast<EndPropertyMapping>().First(p => p.AssociationEnd.Name.EndsWith("_Target"));

                object[] key = propertyMappings.PropertyMappings.Select(c => reader[c.Column.Name]).ToArray();
                object value = context.Set(propertyInfo.PropertyType).Find(key);
                propertyInfo.SetValue(entityObject, value, null);
            }

        }
    }
}
