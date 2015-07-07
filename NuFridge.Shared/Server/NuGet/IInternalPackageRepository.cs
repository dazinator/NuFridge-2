using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public interface IInternalPackageRepository
    {
        int FeedId { get; }

        void RemovePackage(IInternalPackage package);

        void AddPackage(IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion);

        IInternalPackage GetPackage(string packageId, SemanticVersion version);

        IEnumerable<IInternalPackage> GetVersions(ITransaction transaction, string packageId, bool allowPreRelease);

  
        Stream GetPackageRaw(string packageId, SemanticVersion version);

        Stream GetRawContents(IInternalPackage package);

        void IncrementDownloadCount(IInternalPackage package);


    }
}
