using System.Collections.Generic;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Exceptions;

namespace NuFridge.Shared.Database.Services
{
    public class FeedGroupService : IFeedGroupService
    {
        private readonly IFeedGroupRepository _feedGroupRepository;

        public FeedGroupService(IFeedGroupRepository feedGroupRepository)
        {
            _feedGroupRepository = feedGroupRepository;
        }

        public IEnumerable<FeedGroup> GetAll()
        {
            return _feedGroupRepository.GetAll();
        }

        public FeedGroup Find(int id)
        {
            return _feedGroupRepository.Find(id);
        }

        public void Update(FeedGroup feedGroup)
        {
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