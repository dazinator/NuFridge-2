﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Server;
using Palmer;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageRepository : BaseRepository<InternalPackage>, IPackageRepository
    {
        private const string TableName = "Package";

        private const string GetAllPackagesStoredProcCommand = "NuFridge.GetAllPackages @feedId";
        private const string GetLatestPackagesStoredProcCommand = "NuFridge.GetLatestPackages @feedId, @includePrerelease, @partialId";
        private const string GetUniquePackageCountStoredProcCommand = "NuFridge.GetUniquePackageCount @feedId";
        private const string GetVersionsOfPackageStoredProcCommand = "NuFridge.GetVersionsOfPackage @feedId, @includePrerelease, @packageId";
        private const string GetPackageStoredProcCommand = "NuFridge.GetPackage @feedId, @packageId, @version";

        public PackageRepository() : base(TableName)
        {

        }

        public IEnumerable<InternalPackage> GetAllPackagesForFeed(int feedId)
        {
            return Query<InternalPackage>(GetAllPackagesStoredProcCommand, new { feedId });
        }

        public IEnumerable<PackageUpload> GetLatestUploads(int feedId)
        {
            return
                Query<PackageUpload>(
                    $"SELECT TOP(5) [Id], [Version], [Published] FROM [NuFridge].[{TableName}] WHERE FeedId = @feedId ORDER BY Published DESC",
                    new { feedId });
        }


        public void Delete(IEnumerable<int> ids)
        {
            Delete(ids, "Id");
        }

        public int GetCount(int feedId)
        {
            using (var connection = GetConnection())
            {
                return connection.RecordCount<InternalPackage>($"WHERE FeedId = {feedId}");
            }
        }

        public int GetUniquePackageIdCount(int feedId)
        {
            return Query<int>(GetUniquePackageCountStoredProcCommand, new { feedId }).Single();
        }

        public IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId)
        {
            return Query<InternalPackage>(GetLatestPackagesStoredProcCommand, new { feedId, includePrerelease, partialId });
        }

        public IEnumerable<InternalPackage> GetVersionsOfPackage(int? feedId, bool includePrerelease, string packageId)
        {
            return Query<InternalPackage>(GetVersionsOfPackageStoredProcCommand, new { feedId, includePrerelease, packageId });
        }

        public void Insert(InternalPackage package)
        {
            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
            {
                using (var connection = GetConnection())
                {
                    package.PrimaryId = connection.Insert<int>(package);
                }
            });
        }

        public void Update(InternalPackage package)
        {
            Retry.On<SqlException>(
                handle => (handle.Context.LastException as SqlException).Number == Constants.SqlExceptionDeadLockNumber)
                .For(5)
                .With(context =>
                {
                    using (var connection = GetConnection())
                    {
                        connection.Update(package);
                    }
                });
        }

        public InternalPackage GetPackage(int? feedId, string packageId, string version)
        {
            return Query<InternalPackage>(GetPackageStoredProcCommand, new { feedId, packageId, version }).FirstOrDefault();
        }

        public IEnumerable<InternalPackage> GetAllPackagesWithoutAHashOrSize()
        {
            return Query<InternalPackage>($"SELECT * FROM [NuFridge].[{TableName}] WHERE Hash = '' OR PackageSize = 0");
        }
    }

    public interface IPackageRepository
    {
        IEnumerable<InternalPackage> GetAllPackagesForFeed(int feedId);
        void Delete(IEnumerable<int> ids);
        int GetCount(int feedId);
        int GetCount();
        int GetUniquePackageIdCount(int feedId);
        IEnumerable<InternalPackage> GetLatestPackagesForFeed(int feedId, bool includePrerelease, string partialId);
        IEnumerable<InternalPackage> GetVersionsOfPackage(int? feedId, bool includePrerelease, string packageId);
        void Insert(InternalPackage package);
        void Update(InternalPackage package);
        void Delete(InternalPackage package);
        InternalPackage GetPackage(int? feedId, string packageId, string version);
        IEnumerable<InternalPackage> GetAllPackagesWithoutAHashOrSize();
        IEnumerable<PackageUpload> GetLatestUploads(int feedId);
    }
}