using NuFridge.Shared.Database.Services;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class NuGetPackagePathResolver : DefaultPackagePathResolver
    {
        public NuGetPackagePathResolver(IFeedConfigurationService feedConfigurationService, int feedId) : base(GetPackagesFolder(feedConfigurationService, feedId))
        {
            
        }

        private static string GetPackagesFolder(IFeedConfigurationService feedConfigurationService, int feedId)
        {
            return feedConfigurationService.FindByFeedId(feedId).PackagesDirectory;
        }
    }
}