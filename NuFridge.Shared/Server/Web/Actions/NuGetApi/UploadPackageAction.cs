using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApi
{
    public class UploadPackageAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly ILocalFileSystem _fileSystem;
        private readonly IStore _store;

        public UploadPackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, ILocalFileSystem fileSystem,
            IStore store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _fileSystem = fileSystem;
            _store = store;
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

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var feed =
                    transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                feedId = feed.Id;
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

                IInternalPackage latestAbsoluteVersionPackage = null;
                IInternalPackage latestVersionPackage = null;

                var packageRepository = _packageRepositoryFactory.Create(feedId);

                List<IInternalPackage> versionsOfPackage;

                using (ITransaction transaction = _store.BeginTransaction())
                {
                    versionsOfPackage = packageRepository.GetVersions(transaction, package.Id, true).ToList();
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

                bool isUploadedPackageAbsoluteLatestVersion = true;
                bool isUploadedPackageLatestVersion = true;

                if (latestAbsoluteVersionPackage != null)
                {
                    if (package.Version.CompareTo(latestAbsoluteVersionPackage.GetSemanticVersion()) <= 0)
                    {
                        isUploadedPackageAbsoluteLatestVersion = false;
                    }
                }

                if (latestVersionPackage != null)
                {
                    if (package.Version.CompareTo(latestVersionPackage.GetSemanticVersion()) <= 0)
                    {
                        isUploadedPackageLatestVersion = false;
                    }
                    else
                    {
                        if (!package.IsReleaseVersion())
                        {
                            isUploadedPackageLatestVersion = false;
                        }
                    }
                }
                else
                {
                    if (!package.IsReleaseVersion())
                    {
                        isUploadedPackageLatestVersion = false;
                    }
                }


                if (isUploadedPackageAbsoluteLatestVersion && latestAbsoluteVersionPackage != null)
                {
                    latestAbsoluteVersionPackage.IsAbsoluteLatestVersion = false;
                    using (ITransaction transaction = _store.BeginTransaction())
                    {
                        transaction.Update(latestAbsoluteVersionPackage);
                    }
                }

                if (isUploadedPackageLatestVersion && latestVersionPackage != null)
                {
                    latestVersionPackage.IsLatestVersion = false;
                    using (ITransaction transaction = _store.BeginTransaction())
                    {
                        transaction.Update(latestVersionPackage);
                    }
                }

                using (ITransaction transaction = _store.BeginTransaction())
                {
                    transaction.Commit();
                }

                packageRepository.AddPackage(package, isUploadedPackageAbsoluteLatestVersion,
                    isUploadedPackageLatestVersion);

            }
            finally
            {
                if (File.Exists(temporaryFilePath))
                {
                    _fileSystem.DeleteFile(temporaryFilePath);
                }
            }

            return new Response {StatusCode = HttpStatusCode.Created};
        }
    }
}