using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NuFridge.Shared.Logging;

namespace NuFridge.Shared.Server.CommandLine
{
    public static class CommandLineRunner
    {
        private static readonly Encoding OemEncoding;

        static CommandLineRunner()
        {
            try
            {
                Cpinfoex lpCpInfoEx;
                if (GetCPInfoEx(1, 0, out lpCpInfoEx))
                    OemEncoding = Encoding.GetEncoding(lpCpInfoEx.CodePage);
                else
                    OemEncoding = Encoding.GetEncoding(850);
            }
            catch (Exception ex)
            {
                OemEncoding = Encoding.UTF8;
            }
        }

        public static int ExecuteCommand(this CommandLineOptions options, ILog log)
        {
            return options.ExecuteCommand(Environment.CurrentDirectory, log);
        }

        public static int ExecuteCommand(this CommandLineOptions options, string workingDirectory, ILog log)
        {
            string arguments = (options.Arguments ?? "") + " " + (options.SystemArguments ?? "");
            return ExecuteCommand(options.Executable, arguments, workingDirectory, log.Info, log.Error);
        }

        public static CommandLineResult ExecuteCommand(this CommandLineOptions options)
        {
            return options.ExecuteCommand(Environment.CurrentDirectory);
        }

     
        public static CommandLineResult ExecuteCommand(this CommandLineOptions options, string workingDirectory)
        {
            string arguments = (options.Arguments ?? "") + " " + (options.SystemArguments ?? "");
            List<string> infos = new List<string>();
            List<string> errors = new List<string>();
            return new CommandLineResult(ExecuteCommand(options.Executable, arguments, workingDirectory, infos.Add, errors.Add), infos, errors);
        }

        public static int ExecuteCommand(string executable, string arguments, string workingDirectory, Action<string> output, Action<string> error)
        {
            return ExecuteCommand(executable, arguments, workingDirectory, output, error, CancellationToken.None);
        }

        public static int ExecuteCommand(string executable, string arguments, string workingDirectory, Action<string> output, Action<string> error, CancellationToken cancel)
        {
            try
            {
                Process process = new Process();
                try
                {
                    process.StartInfo.FileName = executable;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.WorkingDirectory = workingDirectory;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.StandardOutputEncoding = OemEncoding;
                    process.StartInfo.StandardErrorEncoding = OemEncoding;
                    AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
                    try
                    {
                        AutoResetEvent errorWaitHandle = new AutoResetEvent(false);
                        try
                        {
                            process.OutputDataReceived += (DataReceivedEventHandler)((sender, e) =>
                            {
                                if (e.Data == null)
                                    outputWaitHandle.Set();
                                else
                                    output(e.Data);
                            });
                            process.ErrorDataReceived += (DataReceivedEventHandler)((sender, e) =>
                            {
                                if (e.Data == null)
                                    errorWaitHandle.Set();
                                else
                                    error(e.Data);
                            });
                            process.Start();
                            bool running = true;
                            cancel.Register(() =>
                            {
                                if (!running)
                                    return;
                                DoOurBestToCleanUp(process);
                            });
                            if (cancel.IsCancellationRequested)
                                DoOurBestToCleanUp(process);
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            process.WaitForExit();
                            running = false;
                            outputWaitHandle.WaitOne();
                            errorWaitHandle.WaitOne();
                            return process.ExitCode;
                        }
                        finally
                        {
                            if (errorWaitHandle != null)
                                errorWaitHandle.Dispose();
                        }
                    }
                    finally
                    {
                        if (outputWaitHandle != null)
                            outputWaitHandle.Dispose();
                    }
                }
                finally
                {
                    if (process != null)
                        process.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error when attempting to execute {0}: {1}", executable, ex.Message), ex);
            }
        }

        private static void DoOurBestToCleanUp(Process process)
        {
            try
            {
                KillProcessAndChildren(process.Id);
            }
            catch (Exception ex1)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex2)
                {
                }
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCPInfoEx([MarshalAs(UnmanagedType.U4)] int codePage, [MarshalAs(UnmanagedType.U4)] int dwFlags, out Cpinfoex lpCpInfoEx);

        private static void KillProcessAndChildren(int pid)
        {
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid))
            {
                using (ManagementObjectCollection objectCollection = managementObjectSearcher.Get())
                {
                    foreach (ManagementBaseObject managementBaseObject in objectCollection.OfType<ManagementObject>())
                        KillProcessAndChildren(Convert.ToInt32(managementBaseObject["ProcessID"]));
                }
            }
            try
            {
                Process.GetProcessById(pid).Kill();
            }
            catch (ArgumentException ex)
            {
            }
        }

        private struct Cpinfoex
        {
            [MarshalAs(UnmanagedType.U4)]
            public int MaxCharSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] DefaultChar;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] LeadBytes;
            public char UnicodeDefaultChar;
            [MarshalAs(UnmanagedType.U4)]
            public int CodePage;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string CodePageName;
        }
    }
}
