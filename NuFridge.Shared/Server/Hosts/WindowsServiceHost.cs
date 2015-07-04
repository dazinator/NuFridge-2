using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Extensions;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Hosts
{
    public class WindowsServiceHost : ICommandHost, ICommandRuntime
    {
        private readonly ILog _log = LogProvider.For<WindowsServiceHost>();

        public void Run(Action<ICommandRuntime> start, Action shutdown)
        {
            _log.Trace("Creating the Windows Service host adapter");
            WindowsServiceAdapter windowsServiceAdapter = new WindowsServiceAdapter(() =>
            {
                _log.Trace("Starting the Windows Service");
                start(this);
                _log.Info("The Windows Service has started");
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                FileVersionInfo vi = FileVersionInfo.GetVersionInfo(entryAssembly.Location); 
                _log.InfoFormat("{0} version: {1}",  entryAssembly.GetName().Name,  vi.ProductVersion);
            }, () =>
            {
                _log.Info("Stopping the Windows Service");
                shutdown();
                _log.Info("The Windows Service has stopped");
            });
            _log.Trace("Running the service host adapter");
            ServiceBase.Run(windowsServiceAdapter);
        }

        public void WaitForUserToExit()
        {
        }
    }
}
