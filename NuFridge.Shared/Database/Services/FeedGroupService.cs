using System.Collections.Generic;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

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
    }
}