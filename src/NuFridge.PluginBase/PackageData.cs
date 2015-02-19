using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Lucene;

namespace NuFridge.Service.Plugin
{
    public class PackageData
    {
        public string Id { get; set; }
        public StrictSemanticVersion Version { get; set; }
    }
}