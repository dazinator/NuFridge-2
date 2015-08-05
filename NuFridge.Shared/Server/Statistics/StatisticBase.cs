using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Hangfire;
using Newtonsoft.Json;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Server.Statistics
{
    public abstract class StatisticBase<TModel>
    {
        protected abstract string StatName { get; }

        protected Statistic GetRecord()
        {
            Statistic record;

            using (var dbContext = new DatabaseContext())
            {
                record = dbContext.Statistics.AsNoTracking().FirstOrDefault(rc => rc.Name == StatName);
            }

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

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void UpdateModel(IJobCancellationToken cancellationToken)
        {
            var model = Update();

            Statistic record = GetRecord();

            record.Json = SerializeModel(model);

            using (var dbContext = new DatabaseContext())
            {

                if (record.Id > 0)
                {
                    dbContext.Statistics.Attach(record);
                    dbContext.Entry(record).Property(a => a.Json).IsModified = true;
                }
                else
                {
                    dbContext.Statistics.Add(record);
                }

                cancellationToken.ThrowIfCancellationRequested();

                dbContext.SaveChanges();
            }
        }
    }
}