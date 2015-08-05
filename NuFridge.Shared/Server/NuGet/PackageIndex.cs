using System;
using System.Collections.Generic;
using System.Linq;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class PackageIndex
    {
        private readonly int _feedId;
       // private readonly ICurrentRequest _currentRequest;

        public PackageIndex(int feedId)
        {
            _feedId = feedId;
           // _currentRequest = currentRequest;

            if (feedId <= 0)
            {
                throw new ArgumentException("Feed id is not valid.");
            }
        }

        public void AddPackage(IInternalPackage package)
        {
     //       var username = _currentRequest.Context.CurrentUser.UserName;

            using (var dbContext = new DatabaseContext())
            {
                dbContext.Packages.Add((InternalPackage) package);
                dbContext.SaveChanges();
            }
        }

        public void UnlistPackage(IInternalPackage package)
        {
            InternalPackage internalPackage = (InternalPackage)GetPackage(package.Id, package.GetSemanticVersion());
            if (internalPackage == null)
                return;

            internalPackage.Listed = false;

            using (var dbContext = new DatabaseContext())
            {
                dbContext.Packages.Attach(internalPackage);
                dbContext.Entry(internalPackage).Property(a => a.Listed).IsModified = true;

                dbContext.SaveChanges();
            }
        }

        public void DeletePackage(IInternalPackage package)
        {
            InternalPackage internalPackage = (InternalPackage)GetPackage(package.Id, package.GetSemanticVersion());
            if (internalPackage == null)
                return;

            using (var dbContext = new DatabaseContext())
            {
                dbContext.Packages.Attach(internalPackage);
                dbContext.Packages.Remove(internalPackage);
                dbContext.SaveChanges();
            }
        }

        public IInternalPackage GetPackage(string packageId, SemanticVersion version)
        {
            return LoadPackage(packageId.ToLowerInvariant(), version.ToString().ToLowerInvariant());
        }

        protected virtual IInternalPackage LoadPackage(string id, string version)
        {

            IInternalPackage package;
            using (var dbContext = new DatabaseContext())
            {

                package = EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + _feedId )
                    .FirstOrDefault(pk => pk.FeedId == _feedId && pk.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase) && pk.Version == version);
            }
            return package;
        }

        public void IncrementDownloadCount(IInternalPackage package)
        {
            using (var dbContext = new DatabaseContext())
            {
                IEnumerable<IInternalPackage> packages =
                    EFStoredProcMapper.Map<InternalPackage>(dbContext, dbContext.Database.Connection, "NuFridge.GetAllPackages " + _feedId).Where(
                        pk =>
                            pk.Id.Equals(package.Id, StringComparison.InvariantCultureIgnoreCase) &&
                            pk.FeedId == _feedId);

                var newestPackage = packages.Single(pk => pk.PrimaryId == package.PrimaryId);

                newestPackage.VersionDownloadCount++;

                foreach (var versionOfPackage in packages)
                {
                    versionOfPackage.DownloadCount = packages.Sum(pk => pk.VersionDownloadCount);
                }

                dbContext.SaveChanges();
            }
        }
    }
}