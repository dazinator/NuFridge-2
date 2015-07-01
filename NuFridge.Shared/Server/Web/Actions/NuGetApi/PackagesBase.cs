using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApi
{
    public abstract class PackagesBase
    {
        protected readonly IStore Store;
        protected const string NuGetHeaderApiKeyName = "X-NuGet-ApiKey";
        protected PackagesBase(IStore store)
        {
            Store = store;
        }

        protected void GetNextLatestVersionPackages(int feedId, string packageId, IInternalPackageRepository packageRepository, out IInternalPackage latestAbsoluteVersionPackage, out IInternalPackage latestVersionPackage)
        {
            List<IInternalPackage> versionsOfPackage;

            using (ITransaction transaction = Store.BeginTransaction())
            {
                versionsOfPackage = packageRepository.GetVersions(transaction, packageId, true).ToList();
            }

            latestVersionPackage = versionsOfPackage.Where(vp => !vp.IsPrerelease).Aggregate(versionsOfPackage[0],
                (highest, candiate) =>
                    candiate.GetSemanticVersion().CompareTo(highest.GetSemanticVersion()) > 0 ? candiate : highest);

            latestAbsoluteVersionPackage = versionsOfPackage.Aggregate(versionsOfPackage[0],
                (highest, candiate) =>
                    candiate.GetSemanticVersion().CompareTo(highest.GetSemanticVersion()) > 0 ? candiate : highest);
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
