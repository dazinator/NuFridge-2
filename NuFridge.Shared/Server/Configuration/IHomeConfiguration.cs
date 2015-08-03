namespace NuFridge.Shared.Server.Configuration
{
    public interface IHomeConfiguration
    {
        string InstallDirectory { get;  }
        string ListenPrefixes { get; }
        string NuGetFrameworkNames { get; set; }
        string WindowsDebuggingToolsPath { get;}
        string ConnectionString { get; }
        void Save();
    }
}
