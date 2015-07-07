using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using NuFridge.Shared.Server.Diagnostics.Win;
using NuFridge.Shared.Server.FileSystem;

namespace NuFridge.Shared.Server.Diagnostics
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
        public DateTime StartDate { get; set; }
        public string ExecutablePath { get; set; }
        public uint ThreadCount { get; set; }
        public string WorkingSetSize { get; set; }
        public string ProcessName { get; set; }
        #endregion

        public DateTime LastUpdated { get; set; }
        public string FreeDiskSpace { get; set; }

        [DllImport("kernel32.dll", SetLastError=true, CharSet=CharSet.Auto)]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
   out ulong lpFreeBytesAvailable,
   out ulong lpTotalNumberOfBytes,
   out ulong lpTotalNumberOfFreeBytes);

        public static Win32Process GetProcess()
        {
            var currentProcess = Process.GetCurrentProcess();

            int processId = currentProcess.Id;

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ProcessID =" + processId))
            {
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    return new Win32Process(obj);
                }
            }

            return null;
        }

        public static Win32ComputerSystem GetComputerSystem()
        {
            SelectQuery query = new SelectQuery(@"Select * from Win32_ComputerSystem");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (var obj in searcher.Get().Cast<ManagementObject>())
                {
                    return new Win32ComputerSystem(obj);
                }
            }

            return null;
        }

        public SystemInfo(Win32ComputerSystem system, Win32Process process)
        {
            if (system != null)
            {
                Domain = system.Domain;
                MachineName = system.Name;
                ProcessorCount = system.NumberOfProcessors;
                LogicalProcessorCount = system.NumberOfLogicalProcessors;
                PartOfDomain = system.PartOfDomain;
                SystemType = string.Format("{0} ({1})", system.PcSystemType, system.SystemType);
            }

            if (process != null)
            {
                ProcessName = process.Name;
                ThreadCount = process.ThreadCount;
                ExecutablePath = process.ExecutablePath;
                StartDate = process.CreationDate.ToUniversalTime();

                float workingSetSizeInMb = (process.WorkingSetSize / 1024f) / 1024f;;
                WorkingSetSize = string.Format("{0} MB", workingSetSizeInMb.ToString("0.00"));
            }

            LastUpdated = DateTime.Now;

            ulong freeBytesAvailable;
            ulong totalNumberOfBytes;
            ulong totalNumberOfFreeBytes;

            bool success = GetDiskFreeSpaceEx(Directory.GetCurrentDirectory(),
                                              out freeBytesAvailable,
                                              out totalNumberOfBytes,
                                              out totalNumberOfFreeBytes);

            if (success)
            {
                FreeDiskSpace = BytesToString(freeBytesAvailable);
            }
        }

        private static String BytesToString(ulong byteCount)
        {
            string[] suf = { " B", " KB", " MB", " GB", " TB", " PB", " EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs((long)byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign((long)byteCount) * num).ToString("0.00") + suf[place];
        }
    }
}