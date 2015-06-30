using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApi
{
    public class RedirectToDownloadPackageAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;

        public RedirectToDownloadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
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


            var baseAddress = module.Request.Url.Scheme + "://" + module.Request.Url.HostName + ":" + module.Request.Url.Port + "/feeds/" + feedName;

            var location = string.Format("{0}/packages/{1}/{2}", baseAddress, package.PackageId, package.Version);

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