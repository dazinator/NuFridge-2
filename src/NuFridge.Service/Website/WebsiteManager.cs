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

                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = path;
                watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size | NotifyFilters.LastAccess;
                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;
                watcher.Created += watcher_Changed;
                watcher.Deleted += watcher_Deleted;
                watcher.Renamed += watcher_Renamed;
                watcher.Changed += watcher_Changed;
                watcher.Error += watcher_Error;
                watcher.EnableRaisingEvents = true;
            }
        }

        void watcher_Error(object sender, ErrorEventArgs e)
        {
            Logger.Error("No longer watching website content changes: " + e.GetException());
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
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

                using (var fs = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var fileStream = File.Open(debugContentFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
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

        void watcher_Renamed(object sender, RenamedEventArgs e)
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

                var debugDirectory = Directory.GetCurrentDirectory();
                var debugContentDirectory = Path.Combine(debugDirectory, "Content") + @"\";

                var debugOldContentFilePath = Path.Combine(debugContentDirectory, fileOldRelativePath);
                var debugNewContentFilePath = Path.Combine(debugContentDirectory, fileNewRelativePath);

                File.Move(debugOldContentFilePath, debugNewContentFilePath);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to rename file: " + ex);
            }
        }

        void watcher_Deleted(object sender, FileSystemEventArgs e)
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

                File.Delete(debugContentFilePath);
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to delete file: " + ex);
            }
        }
    }
}