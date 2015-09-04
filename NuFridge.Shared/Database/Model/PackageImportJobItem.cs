using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace NuFridge.Shared.Database.Model
{
    [Table("Job_PackageImportItem", Schema = "NuFridge")]
    public class PackageImportJobItem
    {
        [Key]
        public int Id { get; set; }
        public int JobId { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool Success { get; set; }
        public string JSON { get; set; }
    }
}