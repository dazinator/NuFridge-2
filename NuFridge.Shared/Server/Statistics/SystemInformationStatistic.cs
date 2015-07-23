using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuFridge.Shared.Server.Diagnostics;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Statistics
{
    public class SystemInformationStatistic : StatisticBase<SystemInfo>
    {
        private readonly IServerEngine _engine;

        public SystemInformationStatistic(ITransaction transaction, IServerEngine engine)
            : base(transaction)
        {
            _engine = engine;
        }

        protected override SystemInfo Update()
        {
            var system = SystemInfo.GetComputerSystem();
            var process = SystemInfo.GetProcess();

            var systemInfo = new SystemInfo(system, process, _engine);

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