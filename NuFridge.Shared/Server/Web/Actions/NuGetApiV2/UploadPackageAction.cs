using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class UploadPackageAction : PackagesBase
    {
        private readonly IFeedService _feedService;
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly ILocalFileSystem _fileSystem;
        private readonly ILog _log = LogProvider.For<UploadPackageAction>();
        private readonly IWebPortalConfiguration _portalConfiguration;


        public UploadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, ILocalFileSystem fileSystem, IStore store, IWebPortalConfiguration portalConfiguration, IFeedService feedService)
            : base(store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _fileSystem = fileSystem;
            _portalConfiguration = portalConfiguration;
            _feedService = feedService;
        }

        private dynamic ProcessRequest(string feedName, string filePath, INancyModule module)
        {
            Feed feed = _feedService.Find(feedName, true);

            if (feed == null)
            {
                _log.Warn("Feed does not exist called " + feedName);
                var errorResponse = module.Response.AsText("Feed does not exist.");
                errorResponse.StatusCode = HttpStatusCode.NotFound;

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return errorResponse;
            }

            if (RequiresApiKeyCheck(feed))
            {
                if (module.Request.Headers["Authorization"].FirstOrDefault() != null)
                {
                    try
                    {
                        module.RequiresAuthentication();
                    }
                    catch (Exception)
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        throw;
                    }
                }
                else
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    _log.Warn("Invalid API used to push package for feed " + feed.Name);
                    var errorResponse = module.Response.AsText("Invalid API key.");
                    errorResponse.StatusCode = HttpStatusCode.Forbidden;
                    return errorResponse;
                }
            }

            var response = IndexPackage(filePath, module, feed);

            return response;
        }

        public dynamic UploadFromUrl(int feedId, string filePath, INancyModule module)
        {
            var feed = _feedService.Find(feedId, false);

            return ProcessRequest(feed.Name, filePath, module);
        }

        public dynamic UploadFromFile(dynamic parameters, INancyModule module)
        {
            var file = module.Request.Files.FirstOrDefault();
            string feedName = parameters.feed;

            if (file == null)
            {
                _log.Warn("Must provide package with valid id and version.");
                var errorResponse = module.Response.AsText("Must provide package with valid id and version.");
                errorResponse.StatusCode = HttpStatusCode.NotFound;
                return errorResponse;
            }

            string temporaryFilePath;
            using (var stream = _fileSystem.CreateTemporaryFile(".nupkg", out temporaryFilePath))
            {
                try
                {
                    _log.Debug("Copying the uploaded package to the temp folder at " + temporaryFilePath);

                    file.Value.CopyTo(stream);
                }
                catch (IOException ex)
                {
                    _log.ErrorException("There was an IO error adding the package to the temp folder. " + ex.Message, ex);

                    if (File.Exists(temporaryFilePath))
                    {
                        _log.Info("Deleting the file at " + temporaryFilePath + " as it did not get copied to the temp folder correctly.");
                        File.Delete(temporaryFilePath);
                    }

                    return new TextResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return ProcessRequest(feedName, temporaryFilePath, module);
        }

        private Response IndexPackage(string temporaryFilePath, INancyModule module, Feed feed)
        {
            try
            {
                IFastZipPackage package = FastZipPackage.Open(temporaryFilePath, new CryptoHashProvider());

                package.Listed = true;

                if (string.IsNullOrWhiteSpace(package.Id) || package.Version == null)
                {
                    _log.Warn("Must provide package with valid id and version.");
                    var response = module.Response.AsText("Must provide package with valid id and version.");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                IInternalPackageRepository packageRepository = _packageRepositoryFactory.Create(feed.Id);

                try
                {
                    packageRepository.AddPackage(package);
                }
                catch (PackageConflictException ex)
                {
                    return new TextResponse(HttpStatusCode.Conflict, ex.Message);
                }
                catch (InvalidPackageMetadataException ex)
                {
                    return new TextResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
                catch (Exception ex)
                {
                    return new TextResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            finally
            {
                _fileSystem.DeleteFile(temporaryFilePath);
            }

            return new Response { StatusCode = HttpStatusCode.Created };
        }
    }
}