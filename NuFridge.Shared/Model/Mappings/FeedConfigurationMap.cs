using System.Data;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class FeedConfigurationMap : EntityMapping<IFeedConfiguration>
    {
        public FeedConfigurationMap()
        {
            TableName = "FeedConfiguration";
            Column(col => col.Directory);
            Column(col => col.FeedId);
            Column(col => col.MaxPrereleasePackages);
            Column(col => col.MaxReleasePackages);
            Column(col => col.RetentionPolicyEnabled);
            VirtualColumn("RetentionPolicyDeletePackages", DbType.Boolean, configuration => configuration.RpDeletePackages, (configuration, b) => configuration.RpDeletePackages = b);
        }
    }
}