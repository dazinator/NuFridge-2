namespace NuFridge.Shared.Server.Storage
{
    public class ServerStorageConfiguration : IServerStorageConfiguration
    {
        public string UniqueControllerName { get; set; }

        public string ExternalDatabaseConnectionString { get; set; }
    }
}