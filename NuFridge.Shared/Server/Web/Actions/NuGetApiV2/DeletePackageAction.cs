using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class DeletePackageAction : PackagesBase, IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;

        public DeletePackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store) : base(store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string id = parameters.id;
            string version = parameters.version;
            string feedName = parameters.feed;

            int feedId;
            IFeed feed;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();

                if (feed == null)
                {
                    var response = module.Response.AsText("Feed does not exist.");
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return response;
                }

                feedId = feed.Id;
            }

            if (!IsValidNuGetApiKey(module, feed))
            {
                var response = module.Response.AsText("Invalid API key.");
                response.StatusCode = HttpStatusCode.Forbidden;
                return response;
            }



            var packageRepository = _packageRepositoryFactory.Create(feedId);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


            if (package == null)
            {
                var response = module.Response.AsText(string.Format("Package {0} version {1} not found.", id, version));
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            string deletedPackageId = package.PackageId;
            bool isDeletedPackageLatestVersion = package.IsLatestVersion;
            bool isDeletedPackageAbsoluteLatestVersion = package.IsAbsoluteLatestVersion;

            

            packageRepository.RemovePackage(package);

            if (isDeletedPackageAbsoluteLatestVersion || isDeletedPackageLatestVersion)
            {
                IInternalPackage latestAbsoluteVersionPackage;
                IInternalPackage latestVersionPackage;

                GetNextLatestVersionPackages(feedId, deletedPackageId, packageRepository, out latestAbsoluteVersionPackage, out latestVersionPackage);

                if (latestAbsoluteVersionPackage != null && !latestAbsoluteVersionPackage.IsAbsoluteLatestVersion)
                {
                    latestAbsoluteVersionPackage.IsAbsoluteLatestVersion = true;
                    using (ITransaction transaction = _store.BeginTransaction())
                    {
                        transaction.Update(latestAbsoluteVersionPackage);
                        transaction.Commit();
                    }
                }

                if (latestVersionPackage != null && !latestVersionPackage.IsLatestVersion)
                {
                    latestVersionPackage.IsLatestVersion = true;
                    using (ITransaction transaction = _store.BeginTransaction())
                    {
                        transaction.Update(latestVersionPackage);
                        transaction.Commit();
                    }
                }
            }


            return new Response { StatusCode = HttpStatusCode.Created };
        }
    }
}