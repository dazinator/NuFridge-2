using System;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Web.OData
{
    public class NuGetODataModelBuilderQueryable
    {
        private IEdmModel _model;

        public IEdmModel Model
        {
            get
            {
                if (_model == null)
                {
                    throw new InvalidOperationException("Must invoke Build method before accessing Model.");
                }
                return _model;
            }
        }

        public void Build()
        {
            var builder = new ODataConventionModelBuilder();

            var entity = builder.EntitySet<InternalPackage>("Packages");
            entity.EntityType.HasKey(pkg => pkg.Id);
            entity.EntityType.HasKey(pkg => pkg.Version);

            var searchAction = builder.Action("Search");
            searchAction.Parameter<string>("searchTerm");
            searchAction.Parameter<string>("targetFramework");
            searchAction.Parameter<bool>("includePrerelease");
            searchAction.ReturnsCollectionFromEntitySet<InternalPackage>("Packages");

            var findPackagesAction = builder.Action("FindPackagesById");
            findPackagesAction.Parameter<string>("id");
            findPackagesAction.ReturnsCollectionFromEntitySet<InternalPackage>("Packages");

            var getUpdatesAction = builder.Action("GetUpdates");
            getUpdatesAction.Parameter<string>("packageIds");
            getUpdatesAction.Parameter<bool>("includePrerelease");
            getUpdatesAction.Parameter<bool>("includeAllVersions");
            getUpdatesAction.Parameter<string>("targetFrameworks");
            getUpdatesAction.Parameter<string>("versionConstraints");
            getUpdatesAction.ReturnsCollectionFromEntitySet<InternalPackage>("Packages");

            _model = builder.GetEdmModel();
            _model.SetHasDefaultStream(_model.FindDeclaredType(typeof(InternalPackage).FullName) as IEdmEntityType, true);
        }
    }
}
