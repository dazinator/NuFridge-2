using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Model
{
    public class Framework : IFramework
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public interface IFramework
    {
        int Id { get; set; }
        string Name { get; set; }
    }
}