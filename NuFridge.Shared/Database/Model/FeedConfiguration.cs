using System.IO;
using Dapper;
using NuFridge.Shared.Database.Model.Interfaces;

namespace NuFridge.Shared.Database.Model
{
    [Table("FeedConfiguration", Schema = "NuFridge")]
    public class FeedConfiguration : IFeedConfiguration
    {
        [Key]
        public int Id { get; set; }

        public int FeedId { get; set; }
        public string Directory { get; set; }

        public bool RetentionPolicyEnabled { get; set; }
        public int MaxPrereleasePackages { get; set; }
        public int MaxReleasePackages { get; set; }
        public bool RetentionPolicyDeletePackages { get; set; }

        [Editable(false)]
        public string PackagesDirectory => Path.Combine(Directory, "Packages");
        [Editable(false)]
        public string SymbolsDirectory => Path.Combine(Directory, "Symbols");
    }
}