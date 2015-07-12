using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Security;
using NuFridge.Shared.Model;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class UploadSymbolPackageAction : PackagesBase, IAction
    {
        private readonly IStore _store;
        private readonly ILocalFileSystem _fileSystem;

        public UploadSymbolPackageAction(IStore store, ILocalFileSystem fileSystem)
            : base(store)
        {
            _store = store;
            _fileSystem = fileSystem;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            var file = module.Request.Files.FirstOrDefault();
            string feedName = parameters.feed;

            if (file == null)
            {
                var response = module.Response.AsText("Must provide package with valid id and version.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            int feedId;
            IFeed feed;

            using (ITransaction transaction = _store.BeginTransaction())
            {

                feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();

                if (feed == null)
                {
                    var response = module.Response.AsText("Feed does not exist.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                feedId = feed.Id;
            }

            if (RequiresApiKeyCheck(feed))
            {
                if (!IsValidNuGetApiKey(module, feed))
                {
                    if (module.Request.Headers["Authorization"].FirstOrDefault() != null)
                    {
                        module.RequiresAuthentication();
                    }
                    else
                    {
                        var response = module.Response.AsText("Invalid API key.");
                        response.StatusCode = HttpStatusCode.Forbidden;
                        return response;
                    }
                }
            }

            string temporaryFilePath;
            using (var stream = _fileSystem.CreateTemporaryFile(".nupkg", out temporaryFilePath))
            {
                file.Value.CopyTo(stream);
            }

            try
            {
                IPackage package = FastZipPackage.Open(temporaryFilePath, new CryptoHashProvider());

                if (string.IsNullOrWhiteSpace(package.Id) || package.Version == null)
                {
                    var response = module.Response.AsText("Must provide package with valid id and version.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                if (HasSourceAndSymbols(package))
                {
                    var files = package.GetLibFiles().Where(f => f.Path.EndsWith("pdb", StringComparison.InvariantCultureIgnoreCase));

                }
                else
                {
                    var response = module.Response.AsText("The package that was uploaded is not a valid symbol package.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }
            }
            finally
            {
                _fileSystem.DeleteFile(temporaryFilePath);
            }


            return null;
        }
    }
}