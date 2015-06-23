using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class FeedConfigurationMap : EntityMapping<FeedConfiguration>
    {
        public FeedConfigurationMap()
        {
            Column(col => col.PackagesDirectory);
            Column(col => col.FeedId);
        }
    }
}
