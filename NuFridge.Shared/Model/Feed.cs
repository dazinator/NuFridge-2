using System.Net;
using Newtonsoft.Json;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model
{
    public class Feed : IFeed, IEntity
    {
        public int Id { get; protected set; }

        public string Name { get; set; }

        public string FeedUri { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Feed()
        {
        }

        [JsonConstructor]
        public Feed(string name, string feedUri)
        {
            Name = name;
            FeedUri = feedUri;
            Username = string.Empty;
        }

        public void SetCredentials(string username, string password)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                Username = username;
                Password = password;
            }
            else
            {
                Username = null;
                Password = null;
            }
        }

        public ICredentials GetCredentials()
        {
            if (string.IsNullOrWhiteSpace(Username))
                return CredentialCache.DefaultNetworkCredentials;
            return new NetworkCredential(Username, Password);
        }
    }

    public interface IFeed
    {
        int Id { get; }

        string FeedUri { get; }

        ICredentials GetCredentials();
    }
}
