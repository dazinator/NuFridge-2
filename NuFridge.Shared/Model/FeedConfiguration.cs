using System.IO;
using NuFridge.Shared.Model.Interfaces;

namespace NuFridge.Shared.Model
{
    public class FeedConfiguration : IEntity, IFeedConfiguration
    {
        public FeedConfiguration()
        {

        }

        public int Id { get; set; }

        public int FeedId { get; set; }
        public string Directory { get; set; }

        public bool RetentionPolicyEnabled { get; set; }
        public int MaxPrereleasePackages { get; set; }
        public int MaxReleasePackages { get; set; }
        public bool RpDeletePackages { get; set; }

        public string Name => Id.ToString();


        public string PackagesDirectory => Path.Combine(Directory, "Packages");

        public string SymbolsDirectory => Path.Combine(Directory, "Symbols");
    }
}