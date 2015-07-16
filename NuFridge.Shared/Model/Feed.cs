using System.Net;
using Newtonsoft.Json;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model
{
    public class Feed : IFeed, IEntity
    {
        public int Id { get;  set; }
        public string Name { get; set; }
        public string FeedUri { get; set; }
        public string ApiKey { get; set; }
        public string ApiKeyHashed { get; set; }
        public string ApiKeySalt { get; set; }
        public bool HasApiKey { get; set; }
        public string RootUrl { get; set; }
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
    }
}
