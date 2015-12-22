using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Tests.Database.Repository
{
    public class TestFeedRepository : IFeedRepository
    {
        private List<Feed> _feeds = new List<Feed>();

        public void WithFeeds(List<Feed> feeds)
        {
            _feeds = feeds;
        }

        public void Insert(Feed feed)
        {
            _feeds.Add(feed);
        }

        public IEnumerable<Feed> GetAll()
        {
            return _feeds;
        }

        public Feed Find(int feedId)
        {
            return _feeds.FirstOrDefault(fd => fd.Id == feedId);
        }

        public Feed Find(string feedName)
        {
            return _feeds.FirstOrDefault(fd => fd.Name == feedName);
        }

        public void Delete(Feed feed)
        {
            _feeds.Remove(feed);
        }

        public IEnumerable<Feed> Search(string name)
        {
            return _feeds.Where(fd => fd.Name.Contains(name));
        }

        public int GetCount(bool nolock)
        {
            return _feeds.Count;
        }

        public void Update(Feed feed)
        {
            _feeds[_feeds.FindIndex(fd => fd.Id == feed.Id)] = feed;
        }

        public IEnumerable<Feed> FindByGroupId(int id)
        {
            return _feeds.Where(fd => fd.GroupId == id);
        }
    }
}
