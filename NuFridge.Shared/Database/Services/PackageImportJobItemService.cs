using System;
using System.Collections.Generic;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;
using NuGet;

namespace NuFridge.Shared.Database.Services
{
    public class PackageImportJobItemService : IPackageImportJobItemService
    {
        private readonly IPackageImportJobItemRepository _jobItemRepository;

        public PackageImportJobItemService(IPackageImportJobItemRepository jobItemRepository)
        {
            _jobItemRepository = jobItemRepository;
        }

        public PackageImportJobItem Insert(IPackage package, int jobId)
        {
            PackageImportJobItem jobItem = new PackageImportJobItem
            {
                CreatedAt = DateTime.UtcNow,
                CompletedAt = null,
                StartedAt = null,
                JobId = jobId,
                PackageId = package.Id,
                Version = package.Version.ToString(),
                Success = false
            };

            return _jobItemRepository.Insert(jobItem);
        }

        public IEnumerable<PackageImportJobItem> FindForJob(int jobId)
        {
            return _jobItemRepository.FindForJob(jobId);
        }

        public void Update(PackageImportJobItem item)
        {
            _jobItemRepository.Update(item);
        }
    }

    public interface IPackageImportJobItemService
    {
        PackageImportJobItem Insert(IPackage package, int jobId);
        IEnumerable<PackageImportJobItem> FindForJob(int jobId);
        void Update(PackageImportJobItem item);
    }
}