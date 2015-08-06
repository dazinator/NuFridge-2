﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class DownloadPackageAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IFeedService _feedService;

        public DownloadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IFeedService feedService)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string id = parameters.id;
            string version = parameters.version;
            string feedName = parameters.feed;

            Feed feed = _feedService.Find(id, true);

            if (feed == null)
            {
                var response = module.Response.AsText($"Feed does not exist called {feedName}.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            var packageRepository = _packageRepositoryFactory.Create(feed.Id);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


            if (package == null)
            {
                var response = module.Response.AsText($"Package {id} version {version} not found in {feedName}.");
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