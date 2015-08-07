using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Model;

namespace NuFridge.Shared.Database.Repository
{
    public class PackageDownloadRepository : BaseRepository<PackageDownload>, IPackageDownloadRepository
    {
        private const string TableName = "PackageDownload";

        public PackageDownloadRepository() : base(TableName)
        {
            
        }

        public void Insert(PackageDownload packageDownload)
        {
            using (var connection = GetConnection())
            {
                packageDownload.Id = connection.Insert<int>(packageDownload);
            }
        }
    }

    public interface IPackageDownloadRepository
    {
        void Insert(PackageDownload packageDownload);
    }
}