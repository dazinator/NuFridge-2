using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Owin.Hosting;
using NuFridge.Service.Feeds;
using NuFridge.Service.Logging;

namespace NuFridge.Service.Website
{
    public sealed class WebsiteManager : IDisposable
    {
        private static WebsiteManager _instance;

        protected WebsiteManager()
        {

        }

        public static WebsiteManager Instance()
        {
            if (_instance == null)
            {
                _instance = new WebsiteManager();
            }

            return _instance;
        }

        private static readonly ILog Logger = LogProvider.For<WebsiteManager>();

        private IDisposable WebsiteDisposable { get; set; }

        public void Dispose()
        {


            if (WebsiteDisposable != null)
            {
                Logger.Info("Stopping website.");

                WebsiteDisposable.Dispose();
                WebsiteDisposable = null;
            }
        }

        public static DirectoryInfo FindApplicationFolder(string folderName)
        {
            string startPath = Directory.GetCurrentDirectory();
            DirectoryInfo directory = new DirectoryInfo(startPath);
            while (directory.Name != folderName)
            {
                if (directory.Parent == null)
                {
                    return null;
                }
                directory = directory.Parent;
            }
            return directory;
        }

        public void Start(ServiceConfiguration config)
        {
            Logger.Info("Starting website at " + config.WebsiteBinding + ".");

            string baseAddress = config.WebsiteBinding;

            WebsiteDisposable = WebApp.Start<WebisteStartupConfig>(baseAddress);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Logger.Info("Enabling the website content file watcher.");

                var directory = FindApplicationFolder(Assembly.GetExecutingAssembly().GetName().Name);

                if (directory == null || !directory.Exists)
                {
                    Logger.Warn("Failed to enable the website content file watcher. Could not find the directory to watch.");

                    return;
                }

                var path = Path.Combine(directory.FullName, "Website", "Content");

                if (!Directory.Exists(path))
                {
                    Logger.Warn("Failed to enable the website content file watcher. Could not find the website content folder using " + directory.FullName + ".");

                    return;
                }

                FileSystemWatcher fileWatcher = new FileSystemWatcher();
                fileWatcher.Path = path;
                fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastAccess;
                fileWatcher.Filter = "*.*";
                fileWatcher.IncludeSubdirectories = true;
                fileWatcher.Created += fileWatcher_Changed;
                fileWatcher.Deleted += fileWatcher_Deleted;
                fileWatcher.Renamed += fileWatcher_Renamed;
                fileWatcher.Changed += fileWatcher_Changed;
                fileWatcher.Error += fileWatcher_Error;
                fileWatcher.EnableRaisingEvents = true;

                FileSystemWatcher directoryWatcher = new FileSystemWatcher();
                directoryWatcher.Path = path;
                directoryWatcher.NotifyFilter = NotifyFilters.DirectoryName;
                directoryWatcher.Filter = "*.*";
                directoryWatcher.IncludeSubdirectories = true;
                directoryWatcher.Created += directoryWatcher_Created;
                directoryWatcher.Deleted += directoryWatcher_Deleted;
                directoryWatcher.Renamed += directoryWatcher_Renamed;
                directoryWatcher.Error += directoryWatcher_Error;
                directoryWatcher.EnableRaisingEvents = true;
            }
        }

        private void directoryWatcher_Error(object sender, ErrorEventArgs e)
        {
            Logger.Error("No longer watching website content changes: " + e.GetException());
        }

        private void directoryWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                var srcOldFilePath = e.OldFullPath;
                var srcNewFilePath = e.FullPath;

                if (!Directory.Exists(srcNewFilePath))
                {
                    return;
                }

                var srcDirectory = FindApplicationFolder(Assembly.GetExecutingAssembly().GetName().Name);
                var srcContentDirectory = Path.Combine(srcDirectory.FullName, "Website", "Content") + @"\";

                string fileOldRelativePath = srcOldFilePath.Remove(0, srcContentDirectory.Length);

                if (!srcNewFilePath.Contains(srcContentDirectory))
                {
                    return;
                }

                string fileNewRelativePath = srcNewFilePath.Remove(0, srcContentDirectory.Length);

                var debugDirectory = Directory.GetCurrentDirectory();
                var debugContentDirectory = Path.Combine(debugDirectory, "Content") + @"\";

                var debugOldContentFilePath = Path.Combine(debugContentDirectory, fileOldRelativePath);
                var debugNewContentFilePath = Path.Combine(debugContentDirectory, fileNewRelativePath);

                Directory.Move(debugOldContentFilePath, debugNewContentFilePath);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to rename file: " + ex);
            }
        }

        private void directoryWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                var srcFilePath = e.FullPath;

                var srcDirectory = FindApplicationFolder(Assembly.GetExecutingAssembly().GetName().Name);
                var srcContentDirectory = Path.Combine(srcDirectory.FullName, "Website", "Content") + @"\";

                string fileRelativePath = srcFilePath.Remove(0, srcContentDirectory.Length);

                var debugDirectory = Directory.GetCurrentDirectory();
                var debugContentDirectory = Path.Combine(debugDirectory, "Content") + @"\";

                var debugContentFilePath = Path.Combine(debugContentDirectory, fileRelativePath);

                if (Directory.Exists(debugContentFilePath))
                {
                    Directory.Delete(debugContentFilePath, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to delete directory: " + ex);
            }
        }

        private void directoryWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                var srcFilePath = e.FullPath;

                if (!Directory.Exists(srcFilePath))
                {
                    return;
                }

                var srcDirectory = FindApplicationFolder(Assembly.GetExecutingAssembly().GetName().Name);
                var srcContentDirectory = Path.Combine(srcDirectory.FullName, "Website", "Content") + @"\";

                string fileRelativePath = srcFilePath.Remove(0, srcContentDirectory.Length);

                var debugDirectory = Directory.GetCurrentDirectory();
                var debugContentDirectory = Path.Combine(debugDirectory, "Content") + @"\";

                var debugContentFilePath = Path.Combine(debugContentDirectory, fileRelativePath);
                if (!Directory.Exists(debugContentFilePath))
                {
                    Directory.CreateDirectory(debugContentFilePath);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to create directory: " + ex);
            }
        }

        void fileWatcher_Error(object sender, ErrorEventArgs e)
        {
            Logger.Error("No longer watching website content changes: " + e.GetException());
        }

        void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                var srcFilePath = e.FullPath;

                if (!File.Exists(srcFilePath))
                {
                    return;
                }

                var srcDirectory = FindApplicationFolder(Assembly.GetExecutingAssembly().GetName().Name);
                var srcContentDirectory = Path.Combine(srcDirectory.FullName, "Website", "Content") + @"\";

                string fileRelativePath = srcFilePath.Remove(0, srcContentDirectory.Length);

                var debugDirectory = Directory.GetCurrentDirectory();
                var debugContentDirectory = Path.Combine(debugDirectory, "Content") + @"\";

                var debugContentFilePath = Path.Combine(debugContentDirectory, fileRelativePath);

                var debugNewContentFolderPath = Directory.GetParent(debugContentFilePath);

                if (!debugNewContentFolderPath.Exists)
                {
                    debugNewContentFolderPath.Create();
                }

                using (var fs = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var fileStream = File.Open(debugContentFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fileStream.SetLength(0);
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to change file: " + ex);
            }

        }

        void fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                var srcOldFilePath = e.OldFullPath;
                var srcNewFilePath = e.FullPath;

                if (!File.Exists(srcNewFilePath))
                {
                    return;
                }

                var srcDirectory = FindApplicationFolder(Assembly.GetExecutingAssembly().GetName().Name);
                var srcContentDirectory = Path.Combine(srcDirectory.FullName, "Website", "Content") + @"\";

                string fileOldRelativePath = srcOldFilePath.Remove(0, srcContentDirectory.Length);
                string fileNewRelativePath = srcNewFilePath.Remove(0, srcContentDirectory.Length);


                if (!srcNewFilePath.Contains(srcContentDirectory))
                {
                    return;
                }

                var debugDirectory = Directory.GetCurrentDirectory();
                var debugContentDirectory = Path.Combine(debugDirectory, "Content") + @"\";

                var debugOldContentFilePath = Path.Combine(debugContentDirectory, fileOldRelativePath);
                var debugNewContentFilePath = Path.Combine(debugContentDirectory, fileNewRelativePath);

                var debugNewContentFolderPath = Directory.GetParent(debugNewContentFilePath);

                if (!debugNewContentFolderPath.Exists)
                {
                    debugNewContentFolderPath.Create();
                }

                File.Move(debugOldContentFilePath, debugNewContentFilePath);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to rename file: " + ex);
            }
        }

        void fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                var srcFilePath = e.FullPath;

                var srcDirectory = FindApplicationFolder(Assembly.GetExecutingAssembly().GetName().Name);
                var srcContentDirectory = Path.Combine(srcDirectory.FullName, "Website", "Content") + @"\";

                string fileRelativePath = srcFilePath.Remove(0, srcContentDirectory.Length);

                var debugDirectory = Directory.GetCurrentDirectory();
                var debugContentDirectory = Path.Combine(debugDirectory, "Content") + @"\";

                var debugContentFilePath = Path.Combine(debugContentDirectory, fileRelativePath);

                if (File.Exists(debugContentFilePath))
                {
                    File.Delete(debugContentFilePath);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to delete file: " + ex);
            }
        }
    }
}