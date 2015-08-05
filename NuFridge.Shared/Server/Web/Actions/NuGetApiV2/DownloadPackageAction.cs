using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class DownloadPackageAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;

        public DownloadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string id = parameters.id;
            string version = parameters.version;
            string feedName = parameters.feed;

            int feedId;

            using (var dbContext = new DatabaseContext())
            {
                var feed =
                    dbContext.Feeds.AsNoTracking()
                        .FirstOrDefault(f => f.Name.Equals(feedName, StringComparison.InvariantCultureIgnoreCase));

                if (feed == null)
                {
                    var response = module.Response.AsText("Feed does not exist.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                feedId = feed.Id;
            }

            var packageRepository = _packageRepositoryFactory.Create(feedId);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


            if (package == null)
            {
                var response = module.Response.AsText(string.Format("Package {0} version {1} not found.", id, version));
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            packageRepository.IncrementDownloadCount(package);

            Response result;

            var stream = packageRepository.GetPackageRaw(id, new SemanticVersion(version));

            if (module.Request.Method == HttpMethod.Get.Method)
            {
                result = module.Response.FromStream(stream, "application/zip");

                result.Headers.Add("Content-Length", stream.Length.ToString());
                result.Headers.Add("Content-Disposition", "attachment; filename=" + package.Id + "." + package.Version + ".nupkg");
                using (var md5 = MD5.Create())
                {
                    result.Headers.Add("Content-MD5", BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                    stream.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                result = Response.NoBody;
            }

            result.StatusCode = HttpStatusCode.OK;

            return result;
        }
    }
}