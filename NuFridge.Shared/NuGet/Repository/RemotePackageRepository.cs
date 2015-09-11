using System;
using System.Collections.Generic;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Web.OData;
using Palmer;
using Simple.OData.Client;

namespace NuFridge.Shared.NuGet.Repository
{
    public class RemoteRemotePackageRepository : IRemotePackageRepository
    {
        private readonly ILog _log = LogProvider.For<RemoteRemotePackageRepository>();

        public IEnumerable<PackageImportResult> GetPackages(RemotePackageImportOptions options)
        {
            bool firstRun = true;

            int top = 20;
            int skip = 0;
            long count = 0;

            while (count > skip || firstRun)
            {
                firstRun = false;

                IEnumerable<ODataPackage> packages = QueryFeed(options, top, skip, out count);

                skip += top;

                foreach (var package in packages)
                {
                    yield return new PackageImportResult(package, count);
                }
            }
        }

        private IEnumerable<ODataPackage> QueryFeed(RemotePackageImportOptions options, int top, int skip, out long count)
        {
            var client = new ODataClient(options.FeedUrl);
            var annotations = new ODataFeedAnnotations();

            IEnumerable<ODataPackage> packages = null;

            Retry.On<Exception>().For(3).With(context =>
            {
                _log.Info($"Retrieving the next {top} packages from {options.FeedUrl}");

                packages = client
                    .For<ODataPackage>("Packages")
                    .Top(top)
                    .Skip(skip)
                    .OrderBy(pk => pk.Published)
                    .Select("Id", "Version", "Published")
                    .FindEntriesAsync(annotations).Result;
            });

            if (!annotations.Count.HasValue)
            {
                throw new Exception("Failed to get the total count of packages from the remote feed at " + options.FeedUrl);
            }

            if (packages == null)
            {
                throw new Exception("Failed to retrieve packages from the remote feed at " + options.FeedUrl);
            }

            count = annotations.Count.Value;

            return packages;
        }

        public class PackageImportResult
        {
            public int TotalCount { get; set; }
            public ODataPackage Package { get; set; }

            public PackageImportResult(ODataPackage package, long totalCount)
            {
                Package = package;
                TotalCount = (int)totalCount;
            }
        }
    }

    public interface IRemotePackageRepository
    {
        IEnumerable<RemoteRemotePackageRepository.PackageImportResult> GetPackages(RemotePackageImportOptions options);
    }
}