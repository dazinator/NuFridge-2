using System;

namespace NuFridge.Shared.Commands.Interfaces
{
    public interface ICommandHost
    {
        void Run(Action<ICommandRuntime> start, Action shutdown);
    }
}
