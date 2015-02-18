
using System.Collections.Generic;
using NuGet.Lucene;

namespace NuFridge.Service.Plugin
{
    public interface IPackagePublishReceiver
    {
        void Execute(List<PackagePublishData> packages);
    }
}