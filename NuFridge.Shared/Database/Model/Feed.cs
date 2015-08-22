using System.ComponentModel.DataAnnotations.Schema;
using Dapper;

namespace NuFridge.Shared.Database.Model
{
    [Dapper.Table("Feed", Schema = "NuFridge")]
    public class Feed : IFeed
    {
        [Key]
        public int Id { get;  set; }
        public string Name { get; set; }

        [NotMapped]
        [Editable(false)]
        public string FeedUri { get; set; }

        [NotMapped]
        [Editable(false)]
        public string ApiKey { get; set; }

        public string ApiKeyHashed { get; set; }
        public string ApiKeySalt { get; set; }

        public int GroupId { get; set; }

        [NotMapped]
        [Editable(false)]
        public bool HasApiKey { get; set; }

        [NotMapped]
        [Editable(false)]
        public string RootUrl { get; set; }

        [NotMapped]
        [Editable(false)]
        public string Description { get; set; }
    }

    public interface IFeed
    {
        int Id { get; set; }

        string Name { get; set; }
        string ApiKeyHashed { get; set; }
        string ApiKeySalt { get; set; }
        string FeedUri { get; set; }
        bool HasApiKey { get; set; }
        string ApiKey { get; set; }
        string RootUrl { get; set; }
        string Description { get; set; }
    }
}
