﻿using System.Collections.Generic;
using System.Linq;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using SimpleCrypto;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
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

        protected void GetNextLatestVersionPackages(int feedId, string packageId, IInternalPackageRepository packageRepository, out IInternalPackage latestAbsoluteVersionPackage, out IInternalPackage latestVersionPackage)
        {
            List<IInternalPackage> versionsOfPackage;

            using (ITransaction transaction = Store.BeginTransaction())
            {
                versionsOfPackage = packageRepository.GetVersions(transaction, packageId, true).ToList();
            }

            if (versionsOfPackage.Any())
            {
                latestVersionPackage = versionsOfPackage.Where(vp => !vp.IsPrerelease).Aggregate(versionsOfPackage[0],
                    (highest, candiate) =>
                        candiate.GetSemanticVersion().CompareTo(highest.GetSemanticVersion()) > 0 ? candiate : highest);

                latestAbsoluteVersionPackage = versionsOfPackage.Aggregate(versionsOfPackage[0],
                    (highest, candiate) =>
                        candiate.GetSemanticVersion().CompareTo(highest.GetSemanticVersion()) > 0 ? candiate : highest);
            }
            else
            {
                latestVersionPackage = null;
                latestAbsoluteVersionPackage = null;
            }
        }

        protected void GetCurrentLatestVersionPackages(int feedId, string packageId, IInternalPackageRepository packageRepository, out IInternalPackage latestAbsoluteVersionPackage, out IInternalPackage latestVersionPackage)
        {
            latestAbsoluteVersionPackage = null;
            latestVersionPackage = null;

            List<IInternalPackage> versionsOfPackage;

            using (ITransaction transaction = Store.BeginTransaction())
            {
                versionsOfPackage = packageRepository.GetVersions(transaction, packageId, true).ToList();
            }

            if (versionsOfPackage.Any())
            {
                foreach (var versionOfPackage in versionsOfPackage)
                {
                    if (versionOfPackage.IsAbsoluteLatestVersion)
                    {
                        latestAbsoluteVersionPackage = versionOfPackage;
                    }
                    if (versionOfPackage.IsLatestVersion)
                    {
                        latestVersionPackage = versionOfPackage;
                    }

                    if (latestVersionPackage != null && latestAbsoluteVersionPackage != null)
                    {
                        break;
                    }
                }
            }
        }
    }
}
