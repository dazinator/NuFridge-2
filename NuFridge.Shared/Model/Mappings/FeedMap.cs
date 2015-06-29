using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class FeedMap : EntityMapping<IFeed>
    {
        public FeedMap()
        {
            TableName = "Feed";
            Column(m => m.Name);
            //Column(m => m.FeedUri);
            //Column(m => m.Username);
            //Column(m => m.Password);
            Unique("FeedNameUnique", "Name", "A feed with this name already exists. Please choose a different name.");
        }
    }
}
