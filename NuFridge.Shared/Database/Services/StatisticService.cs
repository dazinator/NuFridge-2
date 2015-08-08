using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IStatisticRepository _statisticRepository;

        public StatisticService(IStatisticRepository statisticRepository)
        {
            _statisticRepository = statisticRepository;
        }

        public Statistic Find(string statName)
        {
            return _statisticRepository.Find(statName);
        }

        public void Insert(Statistic record)
        {
            _statisticRepository.Insert(record);
        }

        public void Update(Statistic record)
        {
            _statisticRepository.Update(record);
        }
    }

    public interface IStatisticService
    {
        Statistic Find(string statName);
        void Insert(Statistic record);
        void Update(Statistic record);
    }
}