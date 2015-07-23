using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Newtonsoft.Json;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public abstract class StatisticBase<TModel>
    {
        protected ITransaction Transaction { get; set; }
        protected abstract string StatName { get; }

        protected StatisticBase(ITransaction transaction)
        {
            Transaction = transaction;
        }

        protected IStatistic GetRecord()
        {
            var record = Transaction.Query<IStatistic>().Where("Name = @name").Parameter("name", StatName).First();

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

            IStatistic record = GetRecord();

            record.Json = SerializeModel(model);

            if (record.Id <= 0)
            {
                Transaction.Insert<IStatistic>(record);
            }
            else
            {
                Transaction.Update<IStatistic>(record);
            }

            cancellationToken.ThrowIfCancellationRequested();

            Transaction.Commit();
        }
    }
}