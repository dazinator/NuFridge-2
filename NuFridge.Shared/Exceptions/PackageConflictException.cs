using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Exceptions
{
    public class PackageConflictException : Exception
    {
        public PackageConflictException(string message) : this(message, null)
        {
            
        }

        public PackageConflictException(string message, Exception ex) : base(message, ex)
        {
            
        }
    }
}