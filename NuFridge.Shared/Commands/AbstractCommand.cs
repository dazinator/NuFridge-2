using System;
using System.Collections.Generic;
using System.IO;
using NuFridge.Shared.Commands.Interfaces;
using NuFridge.Shared.Commands.Options;

namespace NuFridge.Shared.Commands
{
    public abstract class AbstractCommand : ICommand
    {
        private readonly OptionSet _options = new OptionSet();
        private readonly List<ICommandOptions> _optionSets = new List<ICommandOptions>();

        protected OptionSet Options
        {
            get
            {
                return _options;
            }
        }

        protected ICommandRuntime Runtime { get; private set; }

        protected TOptionSet AddOptionSet<TOptionSet>(TOptionSet commandOptions) where TOptionSet : class, ICommandOptions
        {
            if (commandOptions == null)
                throw new ArgumentNullException("commandOptions");
            _optionSets.Add(commandOptions);
            return commandOptions;
        }

        protected virtual void UnrecognizedArguments(IList<string> arguments)
        {
            if (arguments.Count > 0)
                throw new ArgumentException("Unrecognized command line arguments: " + string.Join(" ", arguments));
        }

        protected abstract void Start();

        protected virtual void Stop()
        {
        }

        void ICommand.WriteHelp(TextWriter writer)
        {
            Options.WriteOptionDescriptions(writer);
        }

        void ICommand.Start(string[] commandLineArguments, ICommandRuntime commandRuntime, OptionSet commonOptions)
        {
            Runtime = commandRuntime;
            UnrecognizedArguments(_options.Parse(commandLineArguments));
            foreach (ICommandOptions commandOptions in _optionSets)
                commandOptions.Validate();
            Start();
        }

        void ICommand.Stop()
        {
            Stop();
        }
    }
}
