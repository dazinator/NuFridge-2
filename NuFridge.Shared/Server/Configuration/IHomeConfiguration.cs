namespace NuFridge.Shared.Server.Configuration
{
    public interface IHomeConfiguration
    {
        string InstallDirectory { get;  }
        string SqlDataSource { get;  }
        string SqlUsername { get;  }
        string SqlPassword { get; }
        string SqlInitialCatalog { get; }
        string ListenPrefixes { get; }
    }
}
