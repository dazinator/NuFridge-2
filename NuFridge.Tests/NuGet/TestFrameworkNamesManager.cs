using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet;

namespace NuFridge.Tests.NuGet
{
    public class TestFrameworkNamesManager : FrameworkNamesManager
    {
        protected override void LoadRecordsFromDatabase()
        {
            
        }

        public TestFrameworkNamesManager(IFrameworkService frameworkService) : base(frameworkService)
        {
        }
    }
}