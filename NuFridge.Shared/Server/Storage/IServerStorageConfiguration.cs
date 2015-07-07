namespace NuFridge.Shared.Server.Storage
{
    public interface IServerStorageConfiguration
    {
        string UniqueControllerName { get; set; }

        string ExternalDatabaseConnectionString { get; set; }
    }
}
