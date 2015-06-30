using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApi
{
    public class DeletePackageAction : IAction
    {
        private readonly IInternalPackageRepositoryFactory _packageRepositoryFactory;
        private readonly IStore _store;

        public DeletePackageAction(IInternalPackageRepositoryFactory packageRepositoryFactory, IStore store)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _store = store;
        }

        public dynamic Execute(dynamic parameters, global::Nancy.INancyModule module)
        {
            string id = parameters.id;
            string version = parameters.version;
            string feedName = parameters.feed;

            int feedId;

            using (ITransaction transaction = _store.BeginTransaction())
            {
                var feed = transaction.Query<IFeed>().Where("Name = @feedName").Parameter("feedName", feedName).First();
                feedId = feed.Id;
            }

            var packageRepository = _packageRepositoryFactory.Create(feedId);

            IInternalPackage package = packageRepository.GetPackage(id, new SemanticVersion(version));


            if (package == null)
            {
                var response = module.Response.AsText(string.Format("Package {0} version {1} not found.", id, version));
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            packageRepository.RemovePackage(package);

            return new Response { StatusCode = HttpStatusCode.Created };
        }
    }
}