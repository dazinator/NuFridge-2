using System.IO;
using NuFridge.Shared.Commands.Options;

namespace NuFridge.Shared.Commands.Interfaces
{
    public interface ICommand
    {
        void WriteHelp(TextWriter writer);

        void Start(string[] commandLineArguments, ICommandRuntime commandRuntime, OptionSet commonOptions);

        void Stop();
    }
}
