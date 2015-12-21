using System.Collections.Generic;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet.Repository;
using NuFridge.Shared.Security;
using NuGet;

namespace NuFridge.Shared.Web.Actions.NuGetApiV2
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

            if (module.Context.CurrentUser != null && module.Context.CurrentUser.IsAuthenticated())
            {
                module.RequiresAnyClaim(new List<string> {Claims.SystemAdministrator, Claims.CanDeletePackages});
            }
            else
            {
                if (!IsValidNuGetApiKey(module, feed))
                {
                    var response = module.Response.AsText("You are not authorized to delete packages from this feed.");
                    response.StatusCode = HttpStatusCode.Forbidden;
                    return response;
                }
            }

            var packageRepository = _packageRepositoryFactory.Create(feed.Id);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));

            if (package == null)
            {
                var response = module.Response.AsText($"Package {id} version {version} not found for feed {feedName}.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            if (package.Listed)
            {
                packageRepository.RemovePackage(package);
            }

            return new Response { StatusCode = HttpStatusCode.OK };
        }
    }
}