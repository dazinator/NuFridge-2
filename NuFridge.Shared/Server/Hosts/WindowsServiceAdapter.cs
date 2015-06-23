using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using NuFridge.Shared.Exceptions.Kb;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.Hosts
{
    public class WindowsServiceAdapter : ServiceBase
    {
        private readonly ILog _log = LogProvider.For<WindowsServiceAdapter>();

        private readonly Action _execute;
        private readonly Action _shutdown;
        private Thread _workerThread;

        public WindowsServiceAdapter(Action execute, Action shutdown)
        {
            _execute = execute;
            _shutdown = shutdown;
        }

        protected override void OnStart(string[] args)
        {
            if (args.Length > 0 && args[0].ToLowerInvariant().Contains("debug"))
                Debugger.Launch();
            RequestAdditionalTime(120000);
            _workerThread = new Thread(RunService);
            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        private void RunService()
        {
            try
            {
                _execute();
            }
            catch (Exception ex)
            {
                ExceptionKnowledgeBaseEntry entry;
                if (ExceptionKnowledgeBase.TryInterpret(ex, out entry))
                {
                    string str = entry.ToString();
                    _log.ErrorException(str, ex);
                    throw new Exception(str, ex);
                }
                _log.ErrorException(ex.Message, ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            _shutdown();
        }
    }
}
