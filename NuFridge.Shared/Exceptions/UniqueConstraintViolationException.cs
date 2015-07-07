using System;

namespace NuFridge.Shared.Exceptions
{
    public class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException(string message)
            : base(message)
        {
        }
    }
}
