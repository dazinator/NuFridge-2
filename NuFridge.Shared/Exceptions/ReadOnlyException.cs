using System;

namespace NuFridge.Shared.Exceptions
{
    public class ReadOnlyException : Exception
    {
        public ReadOnlyException() :base("The website is in read-only mode.")
        {
            
        }
    }
}