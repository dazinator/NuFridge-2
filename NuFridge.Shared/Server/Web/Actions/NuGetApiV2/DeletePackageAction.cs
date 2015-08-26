using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class DeletePackageAction : PackagesBase, IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IFeedService _feedService;


        public DeletePackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IFeedService feedService) : base(store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string id = parameters.id;
            string version = parameters.version;
            string feedName = parameters.feed;

            IFeed feed = _feedService.Find(feedName, true);

            if (feed == null)
            {
                var response = module.Response.AsText("Feed does not exist.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (!IsValidNuGetApiKey(module, feed))
            {
                var response = module.Response.AsText("Invalid API key.");
                response.StatusCode = HttpStatusCode.Forbidden;
                return response;
            }

            var packageRepository = _packageRepositoryFactory.Create(feed.Id);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));

            if (package == null)
            {
                var response = module.Response.AsText($"Package {id} version {version} not found for feed {feedName}.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            packageRepository.RemovePackage(package);

            return new Response { StatusCode = HttpStatusCode.OK };
        }
    }
}