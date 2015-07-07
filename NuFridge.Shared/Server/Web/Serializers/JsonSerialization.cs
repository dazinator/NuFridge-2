using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NuFridge.Shared.Server.Web.Serializers
{
    public static class JsonSerialization
    {
        public static JsonSerializerSettings GetDefaultSerializerSettings()
        {
            JsonSerializerSettings serializerSettings1 = new JsonSerializerSettings();
            serializerSettings1.Formatting = (Formatting) 1;
            JsonSerializerSettings serializerSettings2 = serializerSettings1;
            JsonConverterCollection converterCollection1 = new JsonConverterCollection();
            converterCollection1.Add(new StringEnumConverter());
            JsonConverterCollection converterCollection2 = converterCollection1;
            IsoDateTimeConverter dateTimeConverter1 = new IsoDateTimeConverter();
            dateTimeConverter1.DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";
            IsoDateTimeConverter dateTimeConverter2 = dateTimeConverter1;
            converterCollection2.Add(dateTimeConverter2);
            JsonConverterCollection converterCollection3 = converterCollection1;
            serializerSettings2.Converters = converterCollection3;
            return serializerSettings1;
        }
    }
}
