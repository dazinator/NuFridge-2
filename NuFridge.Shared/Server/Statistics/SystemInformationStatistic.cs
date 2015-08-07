using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Diagnostics;
using NuFridge.Shared.Server.Scheduler;

namespace NuFridge.Shared.Server.Statistics
{
    public class SystemInformationStatistic : StatisticBase<SystemInfo>
    {
        private readonly IJobServerManager _jobServerManager;

        public SystemInformationStatistic(IJobServerManager jobServerManager, IStatisticService statisticService) : base(statisticService)
        {
            _jobServerManager = jobServerManager;
        }

        protected override SystemInfo Update()
        {
            var system = SystemInfo.GetComputerSystem();
            var process = SystemInfo.GetProcess();

            var systemInfo = new SystemInfo(system, process, _jobServerManager);

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