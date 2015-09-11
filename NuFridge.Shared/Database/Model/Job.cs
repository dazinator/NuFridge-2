using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Model
{
    [Table("Job", Schema = "NuFridge")]
    public class Job : IJob
    {
        [Key]
        [Editable(true)]
        public int Id { get; set; }
        public int? FeedId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int RetryCount { get; set; }
        public bool Success { get; set; }
        public bool HasWarnings { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
    }

    public interface IJob
    {
        int Id { get; set; }
        int? FeedId { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? CompletedAt { get; set; }
        int RetryCount { get; set; }
        bool Success { get; set; }
        bool HasWarnings { get; set; }
        int UserId { get; set; }
        string Name { get; set; }
    }
}