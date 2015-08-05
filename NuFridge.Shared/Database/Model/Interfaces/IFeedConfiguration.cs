namespace NuFridge.Shared.Database.Model.Interfaces
{
    public interface IFeedConfiguration
    {
        int Id { get; set; }

        int FeedId { get; set; }

        string Directory { get; set; }
        string PackagesDirectory { get; }
        string SymbolsDirectory { get; }
        bool RetentionPolicyDeletePackages { get; set; }
        bool RetentionPolicyEnabled { get; set; }

        int MaxPrereleasePackages { get; set; }
        int MaxReleasePackages { get; set; }
    }
}