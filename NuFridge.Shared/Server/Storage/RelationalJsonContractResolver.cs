using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NuFridge.Shared.Server.Storage
{
  public class RelationalJsonContractResolver : DefaultContractResolver
  {
    private readonly RelationalMappings _mappings;

    public RelationalJsonContractResolver(RelationalMappings mappings)
    {
      _mappings = mappings;
    }

    protected virtual JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
      EntityMapping map;
      _mappings.TryGet(member.DeclaringType, out map);
      JsonProperty property = base.CreateProperty(member, memberSerialization);
      if (property.PropertyName == "Id" && map != null)
        property.Ignored = true;
      if (map != null && map.IndexedColumns.Any(c =>
      {
          if (c.Property != null)
              return c.Property.Name == member.Name;
          return false;
      }))
          property.Ignored = true;
      if (!property.Writable)
      {
        PropertyInfo propertyInfo = member as PropertyInfo;
        if (propertyInfo != null)
        {
          bool flag = propertyInfo.GetSetMethod(true) != null;
            property.Writable = flag;
        }
      }
      return property;
    }
  }
}
