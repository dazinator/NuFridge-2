using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace NuFridge.Shared.Server.NuGet.FastZipPackage
{
    public interface IFastZipPackage : IPackage, IPackageMetadata, IPackageName, IServerPackageMetadata, IDisposable
    {
        new bool Listed
        {
            set;
            get;
        }

        string GetFileLocation();

        Stream GetZipEntryStream(string path);
    }
}
