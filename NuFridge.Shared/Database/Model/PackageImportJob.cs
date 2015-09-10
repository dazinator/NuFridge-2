using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Model
{
    [Table("Job_PackageImport", Schema = "NuFridge")]
    public class PackageImportJob : IPackageImportJob, IJobType
    {
        [Key]
        public int JobId { get; set; }
        public int Processed { get; set; }

        //TODO Rename to Total
        public int Scheduled { get; set; }
        public string JSON { get; set; }
    }

    public interface IPackageImportJob
    {
        int Processed { get; set; }
        int Scheduled { get; set; }
        string JSON { get; set; }
    }
}