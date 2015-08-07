using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Hangfire;
using Newtonsoft.Json;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.Server.Statistics
{
    public abstract class StatisticBase<TModel>
    {
        private readonly IStatisticService _statisticService;
        protected abstract string StatName { get; }


        protected StatisticBase(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        protected Statistic GetRecord()
        {
            Statistic record = _statisticService.Find(StatName);

            bool statExists = record != null;

            if (!statExists)
            {
                record = new Statistic(StatName);
            }

            return record;
        }

        protected abstract TModel Update();

        protected virtual string SerializeModel(TModel model)
        {
            return JsonConvert.SerializeObject(model);
        }

        protected virtual TModel DeserializeModel(string json)
        {
            return JsonConvert.DeserializeObject<TModel>(json);
        }

        public TModel GetModel()
        {
            var record = GetRecord();

            if (string.IsNullOrWhiteSpace(record.Json))
                throw new Exception();

            var model = DeserializeModel(record.Json);

            return model;
        }

        public void UpdateModel(IJobCancellationToken cancellationToken)
        {
            var model = Update();

            Statistic record = GetRecord();

            record.Json = SerializeModel(model);


            if (record.Id > 0)
            {
                _statisticService.Update(record);
            }
            else
            {
                _statisticService.Insert(record);
            }
        }
    }
}