using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Services;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class NuGetFileSystem : PhysicalFileSystem
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