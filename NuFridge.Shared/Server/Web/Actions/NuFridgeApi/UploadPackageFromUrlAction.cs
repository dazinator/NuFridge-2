using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Nancy;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.Security;
using NuFridge.Shared.Server.Web.Actions.NuGetApiV2;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace NuFridge.Shared.Server.Web.Actions.NuFridgeApi
{
    public class UploadPackageFromUrlAction : IAction
    {
        private readonly UploadPackageAction _packageAction;
        private readonly ILocalFileSystem _fileSystem;

        public UploadPackageFromUrlAction(UploadPackageAction packageAction, ILocalFileSystem fileSystem)
        {
            _packageAction = packageAction;
            _fileSystem = fileSystem;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            module.RequiresAnyClaim(new List<string> { Claims.SystemAdministrator, Claims.CanUploadPackages });

            int feed = parameters.id;

            string url = module.Request.Query["url"];

            if (string.IsNullOrEmpty(url))
            {
                return new TextResponse(HttpStatusCode.BadRequest, "No URL provided.");
            }

            Uri uri;

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return new TextResponse(HttpStatusCode.BadRequest, "Invalid URL.");
            }

            string tempPath;

            _fileSystem.CreateTemporaryFile(".nupkg", out tempPath).Dispose();

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(uri, tempPath);
                }
            }
            catch (Exception)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                throw;
            }

            try
            {
                return _packageAction.UploadFromUrl(feed, tempPath, module);
            }
            catch (Exception ex)
            {
                return new TextResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}