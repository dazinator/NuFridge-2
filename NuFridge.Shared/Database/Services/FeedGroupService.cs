using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Exceptions;

namespace NuFridge.Shared.Database.Services
{
    public class FeedGroupService : IFeedGroupService
    {
        private readonly IFeedGroupRepository _feedGroupRepository;
        private readonly IFeedRepository _feedRepository;

        private const string UnassignedFeedsGroupName = "Unassigned Feeds";

        public FeedGroupService(IFeedGroupRepository feedGroupRepository, IFeedRepository feedRepository)
        {
            _feedGroupRepository = feedGroupRepository;
            _feedRepository = feedRepository;
        }

        public IEnumerable<FeedGroup> GetAll()
        {
            return _feedGroupRepository.GetAll();
        }

        public FeedGroup Find(int id)
        {
            return _feedGroupRepository.Find(id);
        }

        public FeedGroup Find(string groupName)
        {
            return _feedGroupRepository.Find(groupName);
        }

        public void Update(FeedGroup feedGroup)
        {
            FeedGroup oldFeedGroup = _feedGroupRepository.Find(feedGroup.Id);
            IEnumerable<int> oldFeedGroupIds = oldFeedGroup.Feeds.Select(oldfd => oldfd.Id);
            IEnumerable<int> newFeedGroupIds = feedGroup.Feeds.Select(fd => fd.Id);

            IEnumerable<Feed> feedsToAddToGroup = feedGroup.Feeds.Where(fd => !oldFeedGroupIds.Contains(fd.Id)).ToList();
            IEnumerable<Feed> feedsToRemoveFromGroup = oldFeedGroup.Feeds.Where(fd => !newFeedGroupIds.Contains(fd.Id)).ToList();

            //TODO handle this scenario better... or change where to store the group id
            if (feedsToRemoveFromGroup.Any())
            {
                FeedGroup unassignedGroup = Find(UnassignedFeedsGroupName);

                if (unassignedGroup == null)
                {
                    unassignedGroup = new FeedGroup {Name = UnassignedFeedsGroupName};
                    Insert(unassignedGroup);
                }

                foreach (var feed in feedsToRemoveFromGroup)
                {
                    _feedRepository.ChangeFeedGroup(feed, unassignedGroup.Id);
                }
            }

            foreach (var feed in feedsToAddToGroup)
            {
                _feedRepository.ChangeFeedGroup(feed, feedGroup.Id);
            }

            _feedGroupRepository.Update(feedGroup);
        }

        public void Insert(FeedGroup feedGroup)
        {
            _feedGroupRepository.Insert(feedGroup);
        }

        public bool Exists(string name)
        {
            var existingGroup = _feedGroupRepository.Find(name);
            if (existingGroup != null)
            {
                return true;
            }

            return false;
        }

        public bool Exists(int groupId)
        {
            var existingGroup = _feedGroupRepository.Find(groupId);
            if (existingGroup != null)
            {
                return true;
            }

            return false;
        }

        public void Delete(FeedGroup feedGroup)
        {
            _feedGroupRepository.Delete(feedGroup);
        }

        public int GetCount()
        {
            return _feedGroupRepository.GetCount(true);
        }
    }

    public interface IFeedGroupService
    {
        IEnumerable<FeedGroup> GetAll();
        int GetCount();
        FeedGroup Find(int id);
        void Update(FeedGroup feedGroup);
        void Insert(FeedGroup feedGroup);
        bool Exists(string name);
        bool Exists(int groupId);
        void Delete(FeedGroup feedGroup);
    }
}