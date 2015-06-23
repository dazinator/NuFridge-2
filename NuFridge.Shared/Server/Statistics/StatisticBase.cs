using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected Statistic GetRecord()
        {
            var record = Transaction.Query<Statistic>().Where("Name = @name").Parameter("name", StatName).First();

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

        public void UpdateModel()
        {
            var model = Update();

            var record = GetRecord();

            record.Json = SerializeModel(model);

            if (record.Id <= 0)
            {
                Transaction.Insert(record);
            }
            else
            {
                Transaction.Update(record);
            }

            Transaction.Commit();
        }
    }
}