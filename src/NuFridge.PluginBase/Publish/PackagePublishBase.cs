using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Service.Plugin.Publish
{
    public abstract class PackagePublishBase
    {
        public abstract void Execute(List<PackageData> packages);

        public abstract void GenerateSettings(SettingConfigurator config);

        public T GetSettingValue<T>(string identifier)
        {
            
        }
    }
}