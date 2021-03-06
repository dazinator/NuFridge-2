﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;

namespace NuFridge.Shared.Server.Web.OData
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

            var entity = builder.EntitySet<IInternalPackage>("Packages");
            entity.EntityType.HasKey(pkg => pkg.Id);
            entity.EntityType.HasKey(pkg => pkg.Version);

            var searchAction = builder.Action("Search");
            searchAction.Parameter<string>("searchTerm");
            searchAction.Parameter<string>("targetFramework");
            searchAction.Parameter<bool>("includePrerelease");
            searchAction.ReturnsCollectionFromEntitySet<IInternalPackage>("Packages");

            var findPackagesAction = builder.Action("FindPackagesById");
            findPackagesAction.Parameter<string>("id");
            findPackagesAction.ReturnsCollectionFromEntitySet<IInternalPackage>("Packages");

            var getUpdatesAction = builder.Action("GetUpdates");
            getUpdatesAction.Parameter<string>("packageIds");
            getUpdatesAction.Parameter<bool>("includePrerelease");
            getUpdatesAction.Parameter<bool>("includeAllVersions");
            getUpdatesAction.Parameter<string>("targetFrameworks");
            getUpdatesAction.Parameter<string>("versionConstraints");
            getUpdatesAction.ReturnsCollectionFromEntitySet<IInternalPackage>("Packages");

            _model = builder.GetEdmModel();
            _model.SetHasDefaultStream(_model.FindDeclaredType(typeof(IInternalPackage).FullName) as IEdmEntityType, true);
        }
    }
}
