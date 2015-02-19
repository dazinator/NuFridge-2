
using System.Collections.Generic;
using NuGet.Lucene;

namespace NuFridge.Service.Plugin
{
    public interface IPackageDownloadReceiver
    {
        void Execute(List<PackageData> packages);
    }
}