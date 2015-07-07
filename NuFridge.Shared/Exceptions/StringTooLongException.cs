using System;

namespace NuFridge.Shared.Exceptions
{
    public class StringTooLongException : Exception
    {
        public StringTooLongException(string message)
            : base(message)
        {
        }
    }
}
