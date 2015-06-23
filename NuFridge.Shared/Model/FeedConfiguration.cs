using NuFridge.Shared.Model.Interfaces;

namespace NuFridge.Shared.Model
{
    public class FeedConfiguration : IEntity
    {
        public virtual int Id { get; set; }

        public virtual int FeedId { get; set; }

        public virtual string PackagesDirectory { get; set; }

        public virtual string Name
        {
            get { return Id.ToString(); }
        }
    }
}