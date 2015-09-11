using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using NuFridge.Shared.Database.Repository;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Scheduler.Jobs;

namespace NuFridge.Shared.Database.Model
{
    [Table("Job_PackageImportItem", Schema = "NuFridge")]
    public class PackageImportJobItem
    {
        private JobExecutionLog _jsonLog;
        private readonly ILog _log = LogProvider.For<PackageImportJobItem>();

        [Key]
        public int Id { get; set; }
        public int JobId { get; set; }
        public string PackageId { get; set; }
        public string Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool Success { get; set; }

        private string _json { get; set; }

        public string JSON
        {
            get
            {
                return _jsonLog?.ToString() ?? _json;
            }
            set
            {
                _json = value;
            }
        }


        public void Log(LogLevel level, string message)
        {
            if (_jsonLog == null)
            {
                _jsonLog = !string.IsNullOrEmpty(JSON)
                    ? JsonConvert.DeserializeObject<JobExecutionLog>(JSON)
                    : new JobExecutionLog();
            }

            switch (level)
            {
                case LogLevel.Trace:
                    _log.Trace(message);
                    break;
                case LogLevel.Debug:
                    _log.Trace(message);
                    break;
                case LogLevel.Info:
                    _log.Trace(message);
                    break;
                case LogLevel.Warn:
                    _log.Trace(message);
                    break;
                case LogLevel.Error:
                    _log.Trace(message);
                    break;
                case LogLevel.Fatal:
                    _log.Trace(message);
                    break;
            }

            _jsonLog.Items.Add(new JobExecutionLog.JobExecutionLogItem {Message = message, Level = level});
        }

        public PackageImportJobItem()
        {

        }
    }
}