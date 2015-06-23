using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Model.Mappings
{
    public class StatisticMap : EntityMapping<Statistic>
    {
        public StatisticMap()
        {
            Column(m => m.Name);
            Column(m => m.Json);
            Unique("StatisticNameUnique", "Name", "A statistic with this name already exists. Please choose a different name.");
        }
    }
}
