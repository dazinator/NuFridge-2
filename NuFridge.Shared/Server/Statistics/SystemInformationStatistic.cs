using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuFridge.Shared.Server.Diagnostics;
using NuFridge.Shared.Server.Scheduler;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class SystemInformationStatistic : StatisticBase<SystemInfo>
    {
        private readonly IJobServer _jobServer;

        public SystemInformationStatistic(ITransaction transaction, IJobServer jobServer)
            : base(transaction)
        {
            _jobServer = jobServer;
        }

        protected override SystemInfo Update()
        {
            var system = SystemInfo.GetComputerSystem();
            var process = SystemInfo.GetProcess();

            var systemInfo = new SystemInfo(system, process, _jobServer);

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