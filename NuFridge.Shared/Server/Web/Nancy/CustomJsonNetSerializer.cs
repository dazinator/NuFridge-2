using Nancy;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;
using NuFridge.Shared.Server.Web.Serializers;

namespace NuFridge.Shared.Server.Web.Nancy
{
    public sealed class CustomJsonNetSerializer : JsonNetSerializer
    {
        public CustomJsonNetSerializer(NancyContext context)
            : base(JsonSerializer.Create(ConfigureSerializationSettings(context)))
        {

        }

        private static JsonSerializerSettings ConfigureSerializationSettings(NancyContext currentRequest)
        {
            JsonSerializerSettings serializerSettings = JsonSerialization.GetDefaultSerializerSettings();

            return serializerSettings;
        }
    }
}
