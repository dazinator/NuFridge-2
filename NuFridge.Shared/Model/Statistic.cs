using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model.Interfaces;

namespace NuFridge.Shared.Model
{
    public class Statistic : IEntity, IStatistic
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Json { get; set; }

        public Statistic()
        {
            
        }

        public Statistic(string name)
        {
            Name = name;
        }
    }

    public interface IStatistic
    {
        int Id { get; set; }

        string Name { get; set; }

        string Json { get; set; }
    }
}