using System;
using System.Collections.Generic;
using System.Text;

namespace NuFridge.Shared.Server.CommandLine
{
    public class CommandLineException : Exception
    {
        private readonly int _exitCode;
        private readonly List<string> _errors;

        public List<string> Errors
        {
            get
            {
                return _errors;
            }
        }

        public override string Message
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(base.Message);
                stringBuilder.AppendFormat(" Exit code: {0}", _exitCode);
                if (Errors.Count > 0)
                    stringBuilder.AppendLine(string.Join(Environment.NewLine, Errors));
                return stringBuilder.ToString();
            }
        }

        public CommandLineException(int exitCode, List<string> errors)
        {
            _exitCode = exitCode;
            _errors = errors;
        }
    }
}
