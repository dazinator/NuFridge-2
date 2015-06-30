using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Nancy;
using NuFridge.Shared.Server.Web.Actions.NuGetApi;

namespace NuFridge.Shared.Server.Web.Modules
{
    public class NuGetApiModule : NancyModule
    {
        public NuGetApiModule(IContainer container)
        {
            Get["feeds/{feed}/api/v2/package/{id}/{version}"] = p => container.Resolve<RedirectToDownloadPackageAction>().Execute(p, this);
            Get["feeds/{feed}/packages/{id}/{version}"] = p => container.Resolve<DownloadPackageAction>().Execute(p, this);
            Post["feeds/{feed}/api/v2/package/{id?}/{version?}"] = p => container.Resolve<UploadPackageAction>().Execute(p, this);
            Post["feeds/{feed}/api/v2/package"] = p => container.Resolve<UploadPackageAction>().Execute(p, this);
            Put["feeds/{feed}/api/v2/package/{id?}/{version?}"] = p => container.Resolve<UploadPackageAction>().Execute(p, this);
            Put["feeds/{feed}/api/v2/package"] = p => container.Resolve<UploadPackageAction>().Execute(p, this);
            Delete["feeds/{feed}/api/v2/package/{id?}/{version?}"] = p => container.Resolve<DeletePackageAction>().Execute(p, this);
            Get["feeds/{feed}/api/v2/Search()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);
            Get["feeds/{feed}/api/v2/odata/Search()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);
            Get["feeds/{feed}/api/v2/Packages()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);
            Get["feeds/{feed}/api/v2/odata/Packages()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);
            Get["feeds/{feed}/api/v2/FindPackagesById()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);
            Get["feeds/{feed}/api/v2/odata/FindPackagesById()"] = p => container.Resolve<GetODataPackagesAction>().Execute(p, this);

            ////Not working
            //Get["feeds/{feed}/api/v2/GetUpdates()"] = p =>
            //{
            //    return null;
            //};

            ////Not working - odata url is for legacy feeds
            //Get["feeds/{feed}/api/v2/odata/GetUpdates()"] = p =>
            //{
            //    return null;
            //};

            ////Not working
            //Get["feeds/{feed}/api/v2/package-ids"] = p => View["index"];

            ////Not working
            //Get["feeds/{feed}/api/v2/package-versions/{packageId}"] = GetPackageVersions(packageRepositoryFactory, store);
        }
    }
}
