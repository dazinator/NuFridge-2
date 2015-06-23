using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model.Interfaces;

namespace NuFridge.Shared.Model
{
    public class Statistic : IEntity
    {
        public int Id { get; protected set; }

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
}