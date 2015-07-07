using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuFridge.Shared.Server.Diagnostics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class SystemInformationStatistic : StatisticBase<SystemInfo>
    {

        public SystemInformationStatistic(ITransaction transaction)
            : base(transaction)
        {

        }

        protected override SystemInfo Update()
        {
            var system = SystemInfo.GetComputerSystem();
            var process = SystemInfo.GetProcess();

            var systemInfo = new SystemInfo(system, process);

            system.Dispose();
            process.Dispose();

            return systemInfo;
        }

        protected override string StatName
        {
            get { return "SystemInformation"; }
        }
    }
}