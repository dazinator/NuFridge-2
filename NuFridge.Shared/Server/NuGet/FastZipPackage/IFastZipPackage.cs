using System;
using System.IO;
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
