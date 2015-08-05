using Autofac;
using Nancy;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class NuGetV1ApiModule : NancyModule
    {
        public NuGetV1ApiModule(IContainer container)
        {
            // VerifyPackageKey
            Get["feeds/{feed}/api/v1/verifykey/{id}/{version}"] = p => null;

            // GetPackageApi
            Get["feeds/{feed}/api/v1/package/{id}/{version}"] = p => null;

            // PushPackageApi
            Get["feeds/{feed}/v1/PackageFiles/{apiKey}/nupkg"] = p => null;

            // DeletePackages
            Get["feeds/{feed}/v1/Packages/{apiKey}/{id}/{version}"] = p => null;

            // PublishPackage
            Get["feeds/{feed}/v1/PublishedPackages/Publish"] = p => null;

            // Legacy GetPackageApi
            Get["feeds/{feed}/v1/Package/Download/{id}/{version}"] = p => null;

            // FeedService
            Get["feeds/{feed}/api/v1/FeedService.svc"] = p => null;

            // FeedService
            Get["feeds/{feed}/v1/FeedService.svc"] = p => null;

            // FeedService
            Get["feeds/{feed}/api/v1"] = p => null;
        }
    }
}