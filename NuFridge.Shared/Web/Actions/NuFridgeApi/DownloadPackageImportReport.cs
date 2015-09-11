using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Newtonsoft.Json;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.FileSystem;
using NuFridge.Shared.Scheduler.Jobs;
using NuGet;
using OpenXmlUtils;

namespace NuFridge.Shared.Web.Actions.NuFridgeApi
{
    public class DownloadPackageImportReport : IAction
    {
        private readonly ILocalFileSystem _fileSystem;
        private readonly IJobService _jobService;
        private readonly IPackageImportJobItemService _packageImportJobItemService;

        public DownloadPackageImportReport(ILocalFileSystem fileSystem, IJobService jobService,
            IPackageImportJobItemService packageImportJobItemService)
        {
            _fileSystem = fileSystem;
            _jobService = jobService;
            _packageImportJobItemService = packageImportJobItemService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            int jobId = parameters.jobid;

            var job = _jobService.Find(jobId);
            var detailedJob = _jobService.Find<PackageImportJob>(jobId);

            var items = _packageImportJobItemService.FindForJob(jobId);

            string tempPath;
            _fileSystem.CreateTemporaryFile(".xlsx", out tempPath).Dispose();

            List<object> rows = new List<object>();

            foreach (var packageImportJobItem in items.Where(it => it.JSON != null))
            {
                var executionLog = JsonConvert.DeserializeObject<JobExecutionLog>(packageImportJobItem.JSON);

                rows.Add(new Dictionary<string, object>
                {
                    {"PackageId", packageImportJobItem.PackageId},
                    {"Version", packageImportJobItem.Version},
                    {"Result", packageImportJobItem.Success ? "Success" : "Failed"},
                    {"StartedAt", packageImportJobItem.StartedAt?.ToString(CultureInfo.InvariantCulture) ?? ""},
                    {"CompletedAt", packageImportJobItem.CompletedAt?.ToString(CultureInfo.InvariantCulture) ?? ""},
                    {"Message", executionLog.Items.Last().Message }
                });

            }

            var fields = new List<SpreadsheetField>
            {
                new SpreadsheetField {Title = "Package Id", FieldName = "PackageId"},
                new SpreadsheetField {Title = "Version", FieldName = "Version"},
                new SpreadsheetField {Title = "Result", FieldName = "Result"},
                new SpreadsheetField {Title = "Started At", FieldName = "StartedAt"},
                new SpreadsheetField {Title = "Completed At", FieldName = "CompletedAt"},
                new SpreadsheetField {Title = "Message", FieldName = "Message"}
            };

            string subTitle =
                $"Processed {detailedJob.Processed} of {detailedJob.Scheduled} packages. Started at {job.CreatedAt}.";

            string fileFriendlyName = Path.GetInvalidFileNameChars().Aggregate(job.Name, (current, c) => current.Replace(c, '-'));

            if (job.CompletedAt.HasValue)
            {
                subTitle += $" Finished at {job.CompletedAt}.";
            }

            Spreadsheet.Create(tempPath,
                new SheetDefinition<object>
                {
                    Fields = fields,
                    Name = "Package Import",
                    IncludeTotalsRow = false,
                    Objects = rows,
                    Title = job.Name,
                    SubTitle = subTitle
                });

            var stream = _fileSystem.OpenFile(tempPath, FileAccess.ReadWrite, FileShare.ReadWrite);

            var response = module.Response.FromStream(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            response.Headers.Add("Content-Length", stream.Length.ToString());
            response.Headers.Add("Content-Disposition", $"attachment; filename={fileFriendlyName}.xlsx");
            using (var md5 = MD5.Create())
            {
                response.Headers.Add("Content-MD5",
                    BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower());
                stream.Seek(0, SeekOrigin.Begin);
            }

            return response;
        }
    }
}