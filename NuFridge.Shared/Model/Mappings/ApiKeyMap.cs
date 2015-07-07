using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class ApiKeyMap : EntityMapping<ApiKey>
    {
        public ApiKeyMap()
        {
            Column(m => m.UserId);
            Column(m => m.ApiKeyHashed);
            Column(m => m.Created);
            Unique("ApiKeyUnique", "ApiKeyHashed", "An error occurred when generating an API key");
        }
    }
}
