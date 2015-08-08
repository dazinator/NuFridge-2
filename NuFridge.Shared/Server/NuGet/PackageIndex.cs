﻿using System;
using Hangfire;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class PackageIndex
    {
        private readonly IPackageService _packageService;
        private readonly IPackageDownloadService _packageDownloadService;
        private readonly int _feedId;
        private readonly object _sync = new object();

        public PackageIndex(IPackageService packageService, IPackageDownloadService packageDownloadService, int feedId)
        {
            _packageService = packageService;
            _packageDownloadService = packageDownloadService;
            _feedId = feedId;

            if (feedId <= 0)
            {
                throw new ArgumentException("Feed id is not valid.");
            }
        }

        public void AddPackage(IInternalPackage package)
        {
            lock (_sync)
            {
                var existingPackage = GetPackage(package.Id, package.GetSemanticVersion());

                if (existingPackage != null)
                {
                    throw new Exception(
                        "A package with the same ID and version already exists. Overwriting packages is not enabled on this feed.");
                }

                _packageService.Insert((InternalPackage) package);
            }
        }

        public void UnlistPackage(IInternalPackage package)
        {
            package.Listed = false;

            _packageService.Update((InternalPackage)package);
        }

        public void DeletePackage(IInternalPackage package)
        {
            _packageService.Delete((InternalPackage)package);
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            return _packageService.GetPackage(_feedId, packageId, version.ToString());
        }

        public void IncrementDownloadCount(IInternalPackage package, string ipAddress, string userAgent)
        {
            //This shouldn't block the download of the package
            BackgroundJob.Enqueue(
                () =>
                    _packageDownloadService.IncrementDownloadCount(_feedId, package.Id, package.Version,
                        DateTime.UtcNow, userAgent, ipAddress));
        }
    }
}