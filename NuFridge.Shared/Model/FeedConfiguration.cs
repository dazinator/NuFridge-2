using NuFridge.Shared.Model.Interfaces;

namespace NuFridge.Shared.Model
{
    public class FeedConfiguration : IEntity
    {
        public virtual int Id { get; set; }

        public virtual int FeedId { get; set; }

        public virtual string PackagesDirectory { get; set; }

        public bool RetentionPolicyEnabled { get; set; }

        public int MaxPrereleasePackages { get; set; }
        public int MaxReleasePackages { get; set; }

        public FeedConfiguration()
        {
            MaxReleasePackages = 10;
            MaxPrereleasePackages = 10;
            RetentionPolicyEnabled = true;
        }

        public virtual string Name
        {
            get { return Id.ToString(); }
        }
    }
}