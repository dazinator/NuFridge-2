using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Autofac;
using Microsoft.Data.OData;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Server.Web.Actions.NuGetApiV2;
using NuFridge.Shared.Server.Web.Batch;
using NuFridge.Shared.Server.Web.OData;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class NuGetApiModule : NancyModule
    {
        public NuGetApiModule(IContainer container)
        {
            //Batch post
            Post["feeds/{feed}/api/v2/$batch"] = p => container.Resolve<BatchAction>().Execute(p, this);

            //Batch post
            Post["feeds/{feed}/api/v2/odata/$batch"] = p => container.Resolve<BatchAction>().Execute(p, this);

            //Redirect to api v2 url
            Get["feeds/{feed}/"] = p => container.Resolve<RedirectToApiV2Action>().Execute(p, this);

            //Redirect to api v2 url
            Get["feeds/{feed}/api"] = p => container.Resolve<RedirectToApiV2Action>().Execute(p, this);

            //Get OData metadata
            Get["feeds/{feed}/api/v2/$metadata"] = p => container.Resolve<GetODataMetadataAction>().Execute(p, this);

            //Get OData metadata
            Get["feeds/{feed}/api/v2/odata/$metadata"] = p => container.Resolve<GetODataMetadataAction>().Execute(p, this);

            //Get OData collections
            Get["feeds/{feed}/api/v2"] = p => container.Resolve<GetODataRootAction>().Execute(p, this);

            //Get OData collections
            Get["feeds/{feed}/api/v2/odata"] = p => container.Resolve<GetODataRootAction>().Execute(p, this);

            //Redirect to package download url
            Get["feeds/{feed}/api/v2/package/{id}/{version}"] = p => container.Resolve<RedirectToDownloadPackageAction>().Execute(p, this);

            //Download package
            Get["feeds/{feed}/packages/{id}/{version}"] = p => container.Resolve<DownloadPackageAction>().Execute(p, this);

            //Upload package
            Post["feeds/{feed}/api/v2/package/{id?}/{version?}"] = p => container.Resolve<UploadPackageAction>().Execute(p, this);

            //Upload package
            Put["feeds/{feed}/api/v2/package/{id?}/{version?}"] = p => container.Resolve<UploadPackageAction>().Execute(p, this);

            //Delete package
            Delete["feeds/{feed}/api/v2/package/{id?}/{version?}"] = p => container.Resolve<DeletePackageAction>().Execute(p, this);

            //OData search
            Get["feeds/{feed}/api/v2/Search()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);

            //OData search
            Get["feeds/{feed}/api/v2/odata/Search()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);

            //OData search count
            Get["feeds/{feed}/api/v2/Search()/$count"] = p => container.Resolve<GetODataPackagesCountAction>().Execute(p, this);

            //OData search count
            Get["feeds/{feed}/api/v2/odata/Search()/$count"] = p => container.Resolve<GetODataPackagesCountAction>().Execute(p, this);

            //OData package listing
            Get["feeds/{feed}/api/v2/Packages()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);

            //OData package listing
            Get["feeds/{feed}/api/v2/odata/Packages()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);

            //OData search count
            Get["feeds/{feed}/api/v2/Packages()/$count"] = p => container.Resolve<GetODataPackagesCountAction>().Execute(p, this);

            //OData search count
            Get["feeds/{feed}/api/v2/odata/Packages()/$count"] = p => container.Resolve<GetODataPackagesCountAction>().Execute(p, this);

            //OData find packages by id
            Get["feeds/{feed}/api/v2/FindPackagesById()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);

            //OData find packages by id
            Get["feeds/{feed}/api/v2/odata/FindPackagesById()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);

            //Package manager console tab completion - package ids
            Get["feeds/{feed}/api/v2/package-ids"] = p => container.Resolve<TabCompletionPackageIdsAction>().Execute(p, this);

            //Package manager console tab completion - package versions
            Get["feeds/{feed}/api/v2/package-versions/{packageId}"] = p => container.Resolve<TabCompletionPackageVersionsAction>().Execute(p, this);

            //OData get updates
            Get["feeds/{feed}/api/v2/GetUpdates()"] = p => container.Resolve<GetUpdatesAction>().Execute(p, this);

            //OData get updates
            Get["feeds/{feed}/api/v2/odata/GetUpdates()"] = p => container.Resolve<GetUpdatesAction>().Execute(p, this);
           
            //OData get updates count
            Get["feeds/{feed}/api/v2/GetUpdates()/$count"] = p => container.Resolve<GetUpdatesCountAction>().Execute(p, this);

            //OData get updates count
            Get["feeds/{feed}/api/v2/odata/GetUpdates()/$count"] = p => container.Resolve<GetUpdatesCountAction>().Execute(p, this);
        }
    }
}
