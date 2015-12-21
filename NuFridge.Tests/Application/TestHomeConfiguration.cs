using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Application;

namespace NuFridge.Tests.Application
{
    public class TestHomeConfiguration : IHomeConfiguration
    {
        public string InstallDirectory => "";
        public string WebsiteDirectory => "";
        public string ListenPrefixes => "";
        public string WindowsDebuggingToolsPath => "";
        public string ConnectionString => "";
        public bool DatabaseReadOnly => false;
    }
}