using System.Collections.Generic;

namespace NuFridge.Shared.Server.CommandLine
{
    public class CommandLineResult
    {
        private readonly int _exitCode;
        private readonly List<string> _infos;
        private readonly List<string> _errors;

        public int ExitCode
        {
            get
            {
                return _exitCode;
            }
        }

        public IList<string> Infos
        {
            get
            {
                return _infos;
            }
        }

        public IList<string> Errors
        {
            get
            {
                return _errors;
            }
        }

        public CommandLineResult(int exitCode, List<string> infos, List<string> errors)
        {
            _exitCode = exitCode;
            _infos = infos;
            _errors = errors;
        }

        public void Validate()
        {
            if (ExitCode != 0)
                throw new CommandLineException(ExitCode, _errors);
        }
    }
}
