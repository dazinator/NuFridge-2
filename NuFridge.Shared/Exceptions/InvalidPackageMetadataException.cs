using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Exceptions
{
    public class InvalidPackageMetadataException : Exception
    {
        public InvalidPackageMetadataException(string message) : this(message, null)
        {

        }

        public InvalidPackageMetadataException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}