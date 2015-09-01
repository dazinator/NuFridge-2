namespace NuFridge.Shared.Server.Configuration
{
    public interface IHomeConfiguration
    {
        string InstallDirectory { get;  }
        string WebsiteDirectory { get; }
        string ListenPrefixes { get; }
        string WindowsDebuggingToolsPath { get;}
        string ConnectionString { get; }
        bool DatabaseReadOnly { get; }
    }
}
