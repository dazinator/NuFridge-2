﻿using System;
using System.Linq;
using Nancy;
using Nancy.Responses;
using Nancy.Security;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuFridge.Shared.Server.Storage;
using NuFridge.Shared.Server.Web.Actions.NuGetApiV2;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.SymbolsApi
{
    public class UploadSymbolPackageAction : PackagesBase, IAction
    {
        private readonly IStore _store;
        private readonly ILocalFileSystem _fileSystem;
        private readonly SymbolSource _symbolSource;
        private readonly IWebPortalConfiguration _portalConfiguration;
        private readonly ILog _log = LogProvider.For<UploadSymbolPackageAction>();

        public UploadSymbolPackageAction(IStore store, ILocalFileSystem fileSystem, SymbolSource symbolSource, IWebPortalConfiguration portalConfiguration)
            : base(store)
        {
            _store = store;
            _fileSystem = fileSystem;
            _symbolSource = symbolSource;
            _portalConfiguration = portalConfiguration;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            var file = module.Request.Files.FirstOrDefault();
            string feedName = parameters.feed;

            if (file == null)
            {
                _log.Warn("Must provide symbol package with valid id and version.");
                var response = module.Response.AsText("Must provide symbol package with valid id and version.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            if (!_symbolSource.Enabled)
            {
                _log.Warn("NuFridge has not been configured to process symbol packages.");
                return new TextResponse(HttpStatusCode.MethodNotAllowed, "NuFridge has not been configured to process symbol packages.");
            }

            IFeed feed;
            IFeedConfiguration config;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
            }


            if (feed == null)
            {
                _log.Warn("Feed does not exist called " + feedName);
                var response = module.Response.AsText("Feed does not exist.");
                response.StatusCode = HttpStatusCode.BadRequest;
                return response;
            }

            using (var transaction = _store.BeginTransaction())
            {
                config =
                    transaction.Query<IFeedConfiguration>()
                        .Where("FeedId = @feedId")
                        .Parameter("feedId", feed.Id)
                        .First();
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
                    _log.Warn("Must provide package with valid id and version.");
                    var response = module.Response.AsText("Must provide package with valid id and version.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                if (HasSourceAndSymbols(package))
                {
                    bool endsWithSlash = _portalConfiguration.ListenPrefixes.EndsWith("/");

                    var symbolUri = $"{_portalConfiguration.ListenPrefixes}{(endsWithSlash ? "" : "/")}feeds/{feed.Name}/api/symbols";

                    _symbolSource.AddSymbolPackage(config, package, symbolUri);
                }
                else
                {
                    _log.Warn("The package that was uploaded is not a valid symbol package.");
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