using System.IO;
using NuFridge.Shared.Database.Model.Interfaces;
using NuGet;

namespace NuFridge.Shared.NuGet.Repository
{
    public interface IInternalPackageRepository
    {
        int FeedId { get; }

        void RemovePackage(IInternalPackage package);

        void AddPackage(IPackage package);

        IInternalPackage GetPackage(string packageId, SemanticVersion version);

        Stream GetPackageRaw(string packageId, SemanticVersion version);

        Stream GetRawContents(IInternalPackage package);

        void IncrementDownloadCount(IInternalPackage package, string ipAddress, string userAgent);


        void DeletePackage(IInternalPackage packageToDelete);
        void IndexPackage(IPackage package);
        string GetPackageFilePath(IInternalPackage package);
    }
}
