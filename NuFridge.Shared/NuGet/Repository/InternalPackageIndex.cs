using System;
using Hangfire;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Exceptions;
using NuGet;

namespace NuFridge.Shared.NuGet.Repository
{
    public class InternalPackageIndex
    {
        private readonly IPackageService _packageService;
        private readonly IPackageDownloadService _packageDownloadService;
        private readonly int _feedId;
        private readonly object _sync = new object();

        public InternalPackageIndex(IPackageService packageService, IPackageDownloadService packageDownloadService, int feedId)
        {
            _packageService = packageService;
            _packageDownloadService = packageDownloadService;
            _feedId = feedId;

            if (feedId <= 0)
            {
                throw new ArgumentException("Feed id is not valid.");
            }
        }

        public void AddPackage(IInternalPackage package, bool allowOverwrite)
        {
            lock (_sync)
            {
                IInternalPackage existingPackage = GetPackage(package.Id, package.GetSemanticVersion());

                if (existingPackage != null)
                {
                    if (!allowOverwrite)
                    {
                        throw new PackageConflictException("This feed does not allow existing packages to be overwritten.");
                    }

                    package.PrimaryId = existingPackage.PrimaryId;

                    _packageService.Update(package);
                }
                else
                {
                    _packageService.Insert(package);
                }
            }
        }

        public void UnlistPackage(IInternalPackage package)
        {
            package.Listed = false;

            lock (_sync)
            {
                _packageService.Update(package);
            }
        }

        public void DeletePackage(IInternalPackage package)
        {
            lock (_sync)
            {
                _packageService.Delete(package);
            }
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            lock (_sync)
            {
                return _packageService.GetPackage(_feedId, packageId, version);
            }
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