using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class FeedConfigurationMap : EntityMapping<IFeedConfiguration>
    {
        public FeedConfigurationMap()
        {
            TableName = "FeedConfiguration";
            Column(col => col.PackagesDirectory);
            Column(col => col.FeedId);
        }
    }
}
