using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Application;

namespace NuFridge.Tests.Application
{
    public class TestApplicationInstanceStore : IApplicationInstanceStore
    {
        public ApplicationInstanceRecord GetInstance()
        {
            ApplicationInstanceRecord record = new ApplicationInstanceRecord
            {
                InstallDirectory = Directory.GetCurrentDirectory()
            };

            return record;
        }
    }
}