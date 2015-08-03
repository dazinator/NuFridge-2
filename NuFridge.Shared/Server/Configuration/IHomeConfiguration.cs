namespace NuFridge.Shared.Server.Configuration
{
    public interface IHomeConfiguration
    {
        string InstallDirectory { get;  }
        string ListenPrefixes { get; }
        string WindowsDebuggingToolsPath { get;}
        string ConnectionString { get; }
    }
}
