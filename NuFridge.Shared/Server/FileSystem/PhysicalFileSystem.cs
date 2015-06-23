using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NuFridge.Shared.Extensions;

namespace NuFridge.Shared.Server.FileSystem
{
    public class PhysicalFileSystem : ILocalFileSystem
    {
        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool DirectoryIsEmpty(string path)
        {
            try
            {
                return !Directory.GetFileSystemEntries(path).Any();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void DeleteFile(string path)
        {
            DeleteFile(path, null);
        }

        public void DeleteFile(string path, DeletionOptions options)
        {
            options = options ?? DeletionOptions.TryThreeTimes;
            if (string.IsNullOrWhiteSpace(path))
                return;
            bool flag = false;
            for (int index = 0; index < options.RetryAttempts; ++index)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        if (flag)
                            File.SetAttributes(path, FileAttributes.Normal);
                        File.Delete(path);
                    }
                }
                catch
                {
                    Thread.Sleep(options.SleepBetweenAttemptsMilliseconds);
                    flag = true;
                    if (index == options.RetryAttempts - 1)
                    {
                        if (!options.ThrowOnFailure)
                            break;
                        throw;
                    }
                }
            }
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public void DeleteDirectory(string path, DeletionOptions options)
        {
            options = options ?? DeletionOptions.TryThreeTimes;
            if (string.IsNullOrWhiteSpace(path))
                return;
            for (int index = 0; index < options.RetryAttempts; ++index)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        directoryInfo.Attributes = directoryInfo.Attributes & ~FileAttributes.ReadOnly;
                        directoryInfo.Delete(true);
                    }
                }
                catch
                {
                    Thread.Sleep(options.SleepBetweenAttemptsMilliseconds);
                    if (index == options.RetryAttempts - 1)
                    {
                        if (!options.ThrowOnFailure)
                            break;
                        throw;
                    }
                }
            }
        }

        public IEnumerable<string> EnumerateFiles(string parentDirectoryPath, params string[] searchPatterns)
        {
            if (searchPatterns.Length != 0)
                return searchPatterns.SelectMany(pattern => Directory.EnumerateFiles(parentDirectoryPath, pattern, SearchOption.TopDirectoryOnly));
            return Directory.EnumerateFiles(parentDirectoryPath, "*", SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> EnumerateFilesRecursively(string parentDirectoryPath, params string[] searchPatterns)
        {
            if (searchPatterns.Length != 0)
                return searchPatterns.SelectMany(pattern => Directory.EnumerateFiles(parentDirectoryPath, pattern, SearchOption.AllDirectories));
            return Directory.EnumerateFiles(parentDirectoryPath, "*", SearchOption.AllDirectories);
        }

        public IEnumerable<string> EnumerateDirectories(string parentDirectoryPath)
        {
            return Directory.EnumerateDirectories(parentDirectoryPath);
        }

        public IEnumerable<string> EnumerateDirectoriesRecursively(string parentDirectoryPath)
        {
            return Directory.EnumerateDirectories(parentDirectoryPath, "*", SearchOption.AllDirectories);
        }

        public long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public void AppendToFile(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }

        public void OverwriteFile(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public Stream OpenFile(string path, FileAccess access, FileShare share)
        {
            return OpenFile(path, FileMode.OpenOrCreate, access, share);
        }

        public Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share);
        }

        public Stream CreateTemporaryFile(string extension, out string path)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;
            path = Path.Combine(GetTempBasePath(), Guid.NewGuid() + extension);
            return OpenFile(path, FileAccess.ReadWrite, FileShare.Read);
        }

        private static string GetTempBasePath()
        {
            string path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name), "Temp");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public string CreateTemporaryDirectory()
        {
            string path = Path.Combine(GetTempBasePath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
            return path;
        }

        public void PurgeDirectory(string targetDirectory, DeletionOptions options)
        {
            PurgeDirectory(targetDirectory, fi => true, options);
        }

        public void PurgeDirectory(string targetDirectory, DeletionOptions options, CancellationToken cancel)
        {
            PurgeDirectory(targetDirectory, fi => true, options, cancel);
        }

        public void PurgeDirectory(string targetDirectory, Predicate<IFileInfo> include, DeletionOptions options)
        {
            PurgeDirectory(targetDirectory, include, options, CancellationToken.None);
        }

        private void PurgeDirectory(string targetDirectory, Predicate<IFileInfo> include, DeletionOptions options, CancellationToken cancel, bool includeTarget = false)
        {
            if (!DirectoryExists(targetDirectory))
                return;
            foreach (string str in EnumerateFiles(targetDirectory))
            {
                cancel.ThrowIfCancellationRequested();
                if (include != null)
                {
                    FileInfoAdapter fileInfoAdapter = new FileInfoAdapter(new FileInfo(str));
                    if (!include(fileInfoAdapter))
                        continue;
                }
                DeleteFile(str, options);
            }
            foreach (string str in EnumerateDirectories(targetDirectory))
            {
                cancel.ThrowIfCancellationRequested();
                if ((new DirectoryInfo(str).Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    Directory.Delete(str);
                else
                    PurgeDirectory(str, include, options, cancel, true);
            }
            if (!includeTarget || !DirectoryIsEmpty(targetDirectory))
                return;
            DeleteDirectory(targetDirectory, options);
        }

        public void OverwriteAndDelete(string originalFile, string temporaryReplacement)
        {
            string str = originalFile + ".backup" + Guid.NewGuid();
            if (!File.Exists(originalFile))
                File.Copy(temporaryReplacement, originalFile, true);
            else
                File.Replace(temporaryReplacement, originalFile, str);
            File.Delete(temporaryReplacement);
            if (!File.Exists(str))
                return;
            File.Delete(str);
        }

        public void WriteAllBytes(string filePath, byte[] data)
        {
            File.WriteAllBytes(filePath, data);
        }

        public string RemoveInvalidFileNameChars(string path)
        {
            char[] invalidChars = Path.GetInvalidPathChars();
            path = new string(path.Where(c => !invalidChars.Contains(c)).ToArray());
            return path;
        }

        public void MoveFile(string sourceFile, string destinationFile)
        {
            File.Move(sourceFile, destinationFile);
        }

        public void EnsureDirectoryExists(string directoryPath)
        {
            if (DirectoryExists(directoryPath))
                return;
            Directory.CreateDirectory(directoryPath);
        }

        public void CopyDirectory(string sourceDirectory, string targetDirectory, int overwriteFileRetryAttempts = 3)
        {
            CopyDirectory(sourceDirectory, targetDirectory, CancellationToken.None, overwriteFileRetryAttempts);
        }

        public void CopyDirectory(string sourceDirectory, string targetDirectory, CancellationToken cancel, int overwriteFileRetryAttempts = 3)
        {
            if (!DirectoryExists(sourceDirectory))
                return;
            if (!DirectoryExists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);
            foreach (string str in Directory.GetFiles(sourceDirectory, "*"))
            {
                cancel.ThrowIfCancellationRequested();
                string targetFile = Path.Combine(targetDirectory, Path.GetFileName(str));
                CopyFile(str, targetFile, overwriteFileRetryAttempts);
            }
            foreach (string str in Directory.GetDirectories(sourceDirectory))
            {
                string fileName = Path.GetFileName(str);
                string targetDirectory1 = Path.Combine(targetDirectory, fileName);
                CopyDirectory(str, targetDirectory1, cancel, overwriteFileRetryAttempts);
            }
        }

        public void CopyFile(string sourceFile, string targetFile, int overwriteFileRetryAttempts = 3)
        {
            for (int index = 0; index < overwriteFileRetryAttempts; ++index)
            {
                try
                {
                    File.Copy(sourceFile, targetFile, true);
                }
                catch
                {
                    Thread.Sleep(1000 + 2000 * index);
                    if (index == overwriteFileRetryAttempts - 1)
                        throw;
                }
            }
        }

        public void EnsureDiskHasEnoughFreeSpace(string directoryPath)
        {
            EnsureDiskHasEnoughFreeSpace(directoryPath, 524288000L);
        }

        public void EnsureDiskHasEnoughFreeSpace(string directoryPath, long requiredSpaceInBytes)
        {
            ulong lpFreeBytesAvailable;
            ulong lpTotalNumberOfBytes;
            ulong lpTotalNumberOfFreeBytes;
            if (!GetDiskFreeSpaceEx(directoryPath, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes))
                return;
            ulong bytes = Math.Max(requiredSpaceInBytes < 0L ? 0UL : (ulong)requiredSpaceInBytes, 524288000UL);
            if (lpTotalNumberOfFreeBytes < bytes)
                throw new IOException(string.Format("The drive containing the directory '{0}' does not have enough free space.", directoryPath));
        }

        public string GetFullPath(string relativeOrAbsoluteFilePath)
        {
            if (!Path.IsPathRooted(relativeOrAbsoluteFilePath))
                relativeOrAbsoluteFilePath = Path.Combine(Environment.CurrentDirectory, relativeOrAbsoluteFilePath);
            relativeOrAbsoluteFilePath = Path.GetFullPath(relativeOrAbsoluteFilePath);
            return relativeOrAbsoluteFilePath;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
    }
}
