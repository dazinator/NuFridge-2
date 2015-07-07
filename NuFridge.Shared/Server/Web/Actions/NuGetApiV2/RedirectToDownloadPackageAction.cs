using System.IO;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class RedirectToDownloadPackageAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;
        private readonly IWebPortalConfiguration _portalConfig;

        public RedirectToDownloadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store, IWebPortalConfiguration portalConfig)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
            _portalConfig = portalConfig;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            string id = parameters.id;
            string version = parameters.version;
            string feedName = parameters.feed;

            int feedId;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                if (feed == null)
                {
                    var errorResponse = module.Response.AsText("Feed does not exist.");
                    errorResponse.StatusCode = HttpStatusCode.BadRequest;
                    return errorResponse;
                }
                feedId = feed.Id;
            }

            var packageRepository = _packageRepositoryFactory.Create(feedId);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


            if (package == null)
            {
                var errorResponse = module.Response.AsText(string.Format("Package {0} version {1} not found.", id, version));
                errorResponse.StatusCode = HttpStatusCode.NotFound;
                return errorResponse;
            }

            var response = new Response();

            bool endsWithSlash = _portalConfig.ListenPrefixes.EndsWith("/");

            var location = string.Format("{0}{1}feeds/{2}/packages/{3}/{4}", _portalConfig.ListenPrefixes, endsWithSlash ? "" : "/", feedName, package.PackageId, package.Version);

            response.Headers.Add("Location", location);

            response.Contents = delegate(Stream stream)
            {
                var writer = new StreamWriter(stream) { AutoFlush = true };
                writer.Write("<html><head><title>Object moved</title></head><body><h2>Object moved to <a href=\"{0}\">here</a>.</h2></body></html>", location);
            };

            response.ContentType = "text/html";
            response.StatusCode = HttpStatusCode.Found;

            return response;
        }
    }
}