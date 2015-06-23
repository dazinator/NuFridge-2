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

        IQueryable<IPackage> GetPackages();

        void RemovePackage(IPackage package);

        void AddPackage(IPackage package, bool isAbsoluteLatestVersion, bool isLatestVersion);

        IPackage GetPackage(string packageId, SemanticVersion version);

        IEnumerable<IPackage> GetVersions(ITransaction transaction, string packageId, bool allowPreRelease);

        List<IPackage> GetPackagesContaining(string searchTerm, out int total, int skip = 0, int take = 30, bool allowPreRelease = true);

        Stream GetPackageRaw(string packageId, SemanticVersion version);

        Stream GetRawContents(IPackage package);

        void IncrementDownloadCount(IPackage package);

        IEnumerable<IPackage> GetWebPackages(ITransaction transaction, string filterType, string filterColumn,
            string filterValue, string orderType, string orderProperty, string searchTerm, string targetFramework, string includePrerelease);
    }
}
