using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Service.Plugin.Settings;

namespace NuFridge.Service.Plugin
{
    public class SettingConfigurator
    {
        public SettingConfigurator AddSetting<T>(T setting) where T : SettingBase
        {
            
        }
    }
}