using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Service.Plugin.Settings
{
    public abstract class SettingBase
    {
        public string DisplayName { get; set; }
        public string Identifier { get; set; }

        protected SettingBase(string identifier, string displayName)
        {
            DisplayName = displayName;
            Identifier = identifier;
        }
    }
}