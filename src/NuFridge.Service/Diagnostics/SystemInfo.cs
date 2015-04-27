using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Service.Diagnostics.Win;

namespace NuFridge.Service.Diagnostics
{
    public class SystemInfo
    {
        #region Win32_ComputerSystem
        public string Domain { get; set; }
        public string MachineName { get; set; }
        public uint LogicalProcessorCount { get; set; }
        public uint ProcessorCount { get; set; }
        public bool PartOfDomain { get; set; }
        public string SystemType { get; set; }
        #endregion

        #region Win32_Process
        public string StartDate { get; set; }
        public string Uptime { get; set; }
        public string ExecutablePath { get; set; }
        public uint ThreadCount { get; set; }
        public string WorkingSetSize { get; set; }
        public string ProcessName { get; set; }
        #endregion

        public SystemInfo(Win32_ComputerSystem system, Win32_Process process)
        {
            if (system != null)
            {
                Domain = system.Domain;
                MachineName = system.Name;
                ProcessorCount = system.NumberOfProcessors;
                LogicalProcessorCount = system.NumberOfLogicalProcessors;
                PartOfDomain = system.PartOfDomain;
                SystemType = string.Format("{0} ({1})", system.PCSystemType, system.SystemType);
            }

            if (process != null)
            {
                ProcessName = process.Name;
                ThreadCount = process.ThreadCount;
                ExecutablePath = process.ExecutablePath;
                StartDate = process.CreationDate.ToString(CultureInfo.CurrentCulture);

                float workingSetSizeInMb = (process.WorkingSetSize / 1024f) / 1024f;;
                WorkingSetSize = string.Format("{0} MB", workingSetSizeInMb.ToString("0.00"));

                TimeSpan diff = DateTime.Now - process.CreationDate;

                string formatted = string.Format("{0}{1}{2}{3}",
                        diff.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", diff.Days, diff.Days == 1 ? String.Empty : "s") : string.Empty,
                        diff.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", diff.Hours, diff.Hours == 1 ? String.Empty : "s") : string.Empty,
                        diff.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", diff.Minutes, diff.Minutes == 1 ? String.Empty : "s") : string.Empty,
                        diff.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", diff.Seconds, diff.Seconds == 1 ? String.Empty : "s") : string.Empty);

                if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

                if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

                Uptime = formatted;
            }
        }
    }
}