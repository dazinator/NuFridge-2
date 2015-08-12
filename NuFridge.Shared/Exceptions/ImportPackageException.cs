using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Exceptions
{
    public class ImportPackageException : Exception
    {
        public ImportPackageException(string message) : base(message)
        {

        }
    }
}