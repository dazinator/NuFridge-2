using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Exceptions
{
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException(string packageId, string version) : base($"The NuGet package was not found on the feed for {packageId} {version}")
        {

        }
    }
}