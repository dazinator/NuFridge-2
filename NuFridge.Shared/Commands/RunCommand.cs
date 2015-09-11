using System;
using NuFridge.Shared.Application;

namespace NuFridge.Shared.Commands
{
    public class RunCommand : AbstractStandardCommand
    {
        private readonly Lazy<IServerEngine> _engine;

        public RunCommand(Lazy<IServerEngine> engine, IApplicationInstanceSelector selector)
            : base(selector)
        {
            _engine = engine;
        }

        protected override void Start()
        {
            base.Start();
            _engine.Value.Start();
            Runtime.WaitForUserToExit();
        }

        protected override void Stop()
        {
            _engine.Value.Stop();
            base.Stop();
        }
    }
}
