using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hangfire;
using NuFridge.Shared.FileSystem;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Scheduler.Jobs.Definitions
{
    [Queue("filesystem")]
    public class CleanupTempFilesJob : JobBase
    {
        public override string JobId => typeof(CleanupTempFilesJob).Name;
        public override bool TriggerOnRegister => true;
        public override string Cron => Hangfire.Cron.Daily(20);

        private readonly ILog _log = LogProvider.For<CleanupTempFilesJob>();
        private readonly string[] _sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        [AutomaticRetry(Attempts = 0)]
        [DisableConcurrentExecution(10)]
        public override void Execute(IJobCancellationToken cancellationToken)
        {
            _log.Info("Executing " + JobId + " job");

            var expiryDate = DateTime.UtcNow.AddHours(-24);

            string tempDirectory = PhysicalFileSystem.GetTempBasePath();

            _log.Info("Checking for old temporary files in " + tempDirectory);

            long freeSpaceCleared = 0;

            if (Directory.Exists(tempDirectory))
            {
                List<string> allDirectories = Directory.GetDirectories(tempDirectory, "*", SearchOption.AllDirectories).Reverse().ToList();

                allDirectories.Add(tempDirectory);

                foreach (var directoryPath in allDirectories)
                {
                    string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly);

                    int i = 0;

                    foreach (var file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.LastAccessTimeUtc <= expiryDate)
                            {
                                long size = fi.Length;

                                try
                                {
                                    fi.Delete();
                                    i++;
                                    freeSpaceCleared += size;
                                }
                                catch (Exception ex)
                                {
                                    _log.WarnException("Failed to delete temp file " + file, ex);
                                }
                            }
                        }
                    }

                    if (i == files.Length && tempDirectory != directoryPath)
                    {
                        try
                        {
                            Directory.Delete(directoryPath, false);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }

            if (freeSpaceCleared > 0)
            {
                _log.Info($"Cleared {SizeSuffix(freeSpaceCleared)} of disk space used for old temporary files");
            }
        }

        string SizeSuffix(long value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return $"{adjustedSize:n1} {_sizeSuffixes[mag]}";
        }
    }
}