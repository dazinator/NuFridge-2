using System;

namespace NuFridge.Shared.Exceptions
{
    public class SecurityException : Exception
    {
        public string HelpText { get; set; }

        public SecurityException(string message)
            : base(message)
        {
        }
    }
}
