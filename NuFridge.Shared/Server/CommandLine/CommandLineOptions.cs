namespace NuFridge.Shared.Server.CommandLine
{
    public class CommandLineOptions
    {
        private readonly string _executable;
        private readonly string _arguments;
        private readonly string _systemArguments;

        public string Executable
        {
            get
            {
                return _executable;
            }
        }

        public string Arguments
        {
            get
            {
                return _arguments;
            }
        }

        public string SystemArguments
        {
            get
            {
                return _systemArguments;
            }
        }

        public bool IgnoreFailedExitCode { get; set; }

        public CommandLineOptions(string executable, string arguments, string systemArguments)
        {
            _executable = executable;
            _arguments = arguments;
            _systemArguments = systemArguments;
        }

        public override string ToString()
        {
            return "\"" + _executable + "\" " + _arguments;
        }
    }
}
