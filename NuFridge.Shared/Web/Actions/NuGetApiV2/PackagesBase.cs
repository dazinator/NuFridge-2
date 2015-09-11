﻿using System;
using System.IO;
using System.Linq;
using Nancy;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Model;
using NuGet;
using SimpleCrypto;

namespace NuFridge.Shared.Web.Actions.NuGetApiV2
{
    public abstract class PackagesBase
    {
        protected readonly IStore Store;
        protected const string NuGetHeaderApiKeyName = "X-NuGet-ApiKey";
        protected PackagesBase(IStore store)
        {
            Store = store;
        }

        protected bool RequiresApiKeyCheck(IFeed feed)
        {
            return !string.IsNullOrWhiteSpace(feed.ApiKeyHashed);
        }

        protected bool HasSourceAndSymbols(IPackage package)
        {
            var hasSymbols = package.GetFiles()
                .Any(pf => string.Equals(Path.GetExtension(pf.Path), ".pdb",
                    StringComparison.InvariantCultureIgnoreCase));

            return hasSymbols && package.GetFiles("src").Any();
        }

        protected bool IsValidNuGetApiKey(INancyModule module, IFeed feed)
        {
            if (!string.IsNullOrWhiteSpace(feed.ApiKeyHashed))
            {
                if (module.Request.Headers[NuGetHeaderApiKeyName].FirstOrDefault() == null)
                {
                    return false;
                }

                ICryptoService cryptoService = new PBKDF2();

                var feedApiKeyHashed = feed.ApiKeyHashed;
                var feedApiKeySalt = feed.ApiKeySalt;

                var requestApiKey = module.Request.Headers[NuGetHeaderApiKeyName].FirstOrDefault();

                if (string.IsNullOrWhiteSpace(requestApiKey))
                {
                    return false;
                }

                string requestApiKeyHashed = cryptoService.Compute(requestApiKey, feedApiKeySalt);
                bool isValidApiKey = cryptoService.Compare(requestApiKeyHashed, feedApiKeyHashed);

                if (!isValidApiKey)
                {
                    return false;
                }
            }

            return true;
        }
    }
}