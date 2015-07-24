﻿using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class UploadPackageAction : PackagesBase, IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly ILocalFileSystem _fileSystem;
        private readonly ILog _log = LogProvider.For<UploadPackageAction>();


        public UploadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, ILocalFileSystem fileSystem, IStore store)
            : base(store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _fileSystem = fileSystem;
        }

        public dynamic Execute(int feedId, string filePath, INancyModule module)
        {
            IFeed feed;

            using (ITransaction transaction = Store.BeginTransaction())
            {

                feed = transaction.Load<IFeed>(feedId);

                if (feed == null)
                {
                    var errorResponse = module.Response.AsText("Feed does not exist.");
                    errorResponse.StatusCode = HttpStatusCode.BadRequest;
                    return errorResponse;
                }

                feedId = feed.Id;
            }

            if (RequiresApiKeyCheck(feed))
            {
                if (module.Request.Headers["Authorization"].FirstOrDefault() != null)
                {
                    module.RequiresAuthentication();
                }
                else
                {
                    var errorResponse = module.Response.AsText("Invalid API key.");
                    errorResponse.StatusCode = HttpStatusCode.Forbidden;
                    return errorResponse;
                }
            }

            var response = ProcessPackage(filePath, module, feedId);

            return response;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            var file = module.Request.Files.FirstOrDefault();
            string feedName = parameters.feed;

            if (file == null)
            {
                var errorResponse = module.Response.AsText("Must provide package with valid id and version.");
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                return errorResponse;
            }

            int feedId;
            IFeed feed;

            using (ITransaction transaction = Store.BeginTransaction())
            {

                feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();

                if (feed == null)
                {
                    var errorResponse = module.Response.AsText("Feed does not exist.");
                    errorResponse.StatusCode = HttpStatusCode.BadRequest;
                    return errorResponse;
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
                        var errorResponse = module.Response.AsText("Invalid API key.");
                        errorResponse.StatusCode = HttpStatusCode.Forbidden;
                        return errorResponse;
                    }
                }
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

            var response = ProcessPackage(temporaryFilePath, module, feedId);

            return response;
        }

        private Response ProcessPackage(string temporaryFilePath, INancyModule module, int feedId)
        {
            try
            {
                IPackage package = FastZipPackage.Open(temporaryFilePath, new CryptoHashProvider());

                if (string.IsNullOrWhiteSpace(package.Id) || package.Version == null)
                {
                    var response = module.Response.AsText("Must provide package with valid id and version.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                IInternalPackageRepository packageRepository = _packageRepositoryFactory.Create(feedId);

                var existingPackage = packageRepository.GetPackage(package.Id, package.Version);

                if (existingPackage != null)
                {
                    var response = module.Response.AsText("A package with the same ID and version already exists. Overwriting packages is not enabled on this feed.");
                    response.StatusCode = HttpStatusCode.Conflict;
                    return response;
                }

                bool isUploadedPackageLatestVersion;
                bool isUploadedPackageAbsoluteLatestVersion;
                UpdateLatestVersionFlagsForPackageId(feedId, package, packageRepository, out isUploadedPackageLatestVersion, out isUploadedPackageAbsoluteLatestVersion);

                try
                {
                    packageRepository.AddPackage(package, isUploadedPackageAbsoluteLatestVersion, isUploadedPackageLatestVersion);
                }
                catch (IOException ex)
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