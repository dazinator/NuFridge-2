using System;
using System.Collections.Generic;
using Hangfire;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Services
{
    [Queue("download")]
    [AutomaticRetry(Attempts = 10, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class PackageDownloadService : IPackageDownloadService
    {
        private readonly IPackageDownloadRepository _packageDownloadRepository;

        public PackageDownloadService(IPackageDownloadRepository packageDownloadRepository)
        {
            _packageDownloadRepository = packageDownloadRepository;
        }

        public void IncrementDownloadCount(int feedId, string packageId, string version, DateTime downloadedAt,
            string userAgent, string ipAddress)
        {
            PackageDownload packageDownload = new PackageDownload
            {
                FeedId = feedId,
                PackageId = packageId,
                Version = version,
                DownloadedAt = downloadedAt,
                UserAgent = userAgent,
                IPAddress = ipAddress
            };

            _packageDownloadRepository.Insert(packageDownload);
        }

        public IEnumerable<PackageDownload> GetLatestDownloads(int feedId)
        {
            return _packageDownloadRepository.GetLatestDownloads(feedId);
        }

        public int GetCount()
        {
            return _packageDownloadRepository.GetCount(true);
        }
    }

    public interface IPackageDownloadService
    {
        void IncrementDownloadCount(int feedId, string packageId, string version, DateTime downloadedAt, string userAgent, string ipAddress);
        IEnumerable<PackageDownload> GetLatestDownloads(int feedId);
        int GetCount();
    }
}