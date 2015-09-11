using NuFridge.Shared.Database.Services;

namespace NuFridge.Shared.FileSystem
{
    public class NuGetFileSystem : global::NuGet.PhysicalFileSystem
    {
        public NuGetFileSystem(IFeedConfigurationService feedConfigurationService, int feedId) : base(GetPackagesFolder(feedConfigurationService, feedId))
        {

        }

        private static string GetPackagesFolder(IFeedConfigurationService feedConfigurationService, int feedId)
        {
            return feedConfigurationService.FindByFeedId(feedId).PackagesDirectory;
        }
    }
}