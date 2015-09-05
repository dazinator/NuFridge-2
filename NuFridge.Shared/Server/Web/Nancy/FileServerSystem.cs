using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using Microsoft.Owin.FileSystems;

namespace NuFridge.Shared.Server.Web.Nancy
{
    public class FileServerSystem : IFileSystem
    {
        private const string CacheKey = "FileServerSystem:";
        private readonly bool _useCache;

        private static readonly Dictionary<string, string> RestrictedFileNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "con",
                    string.Empty
                },
                {
                    "prn",
                    string.Empty
                },
                {
                    "aux",
                    string.Empty
                },
                {
                    "nul",
                    string.Empty
                },
                {
                    "com1",
                    string.Empty
                },
                {
                    "com2",
                    string.Empty
                },
                {
                    "com3",
                    string.Empty
                },
                {
                    "com4",
                    string.Empty
                },
                {
                    "com5",
                    string.Empty
                },
                {
                    "com6",
                    string.Empty
                },
                {
                    "com7",
                    string.Empty
                },
                {
                    "com8",
                    string.Empty
                },
                {
                    "com9",
                    string.Empty
                },
                {
                    "lpt1",
                    string.Empty
                },
                {
                    "lpt2",
                    string.Empty
                },
                {
                    "lpt3",
                    string.Empty
                },
                {
                    "lpt4",
                    string.Empty
                },
                {
                    "lpt5",
                    string.Empty
                },
                {
                    "lpt6",
                    string.Empty
                },
                {
                    "lpt7",
                    string.Empty
                },
                {
                    "lpt8",
                    string.Empty
                },
                {
                    "lpt9",
                    string.Empty
                },
                {
                    "clock$",
                    string.Empty
                }
            };

        public string Root { get; }

        public FileServerSystem(string root)
        {
            Root = GetFullRoot(root);
            if (!Directory.Exists(Root))
                throw new DirectoryNotFoundException(Root);

#if (DEBUG)
            _useCache = false;
#endif
        }

        private static string GetFullRoot(string root)
        {
            string fullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, root));
            if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                fullPath += Path.DirectorySeparatorChar;
            return fullPath;
        }

        private string GetFullPath(string path)
        {
            string fullPath = Path.GetFullPath(Path.Combine(Root, path));
            if (!fullPath.StartsWith(Root, StringComparison.OrdinalIgnoreCase))
                return null;
            return fullPath;
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            try
            {
                if (subpath.StartsWith("/", StringComparison.Ordinal))
                    subpath = subpath.Substring(1);
                string fullPath = GetFullPath(subpath);

                if (fullPath != null)
                {
                    if (_useCache)
                    {
                        var cachedItem = MemoryCache.Default.Get(CacheKey + subpath);
                        if (cachedItem != null)
                        {
                            fileInfo = (PhysicalFileInfo) cachedItem;
                            return true;
                        }
                    }

                    FileInfo fileInfo1 = new FileInfo(fullPath);
                    if (fileInfo1.Exists)
                    {
                        if (!IsRestricted(fileInfo1))
                        {
                            fileInfo = new PhysicalFileInfo(fileInfo1);
                            if (_useCache)
                            {
                                CacheItemPolicy policy = new CacheItemPolicy {SlidingExpiration = TimeSpan.FromDays(1)};
                                MemoryCache.Default.Set(CacheKey + subpath, fileInfo, policy);
                            }
                            return true;
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
            }
            fileInfo = null;
            return false;
        }

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            try
            {
                if (subpath.StartsWith("/", StringComparison.Ordinal))
                    subpath = subpath.Substring(1);
                string fullPath = GetFullPath(subpath);
                if (fullPath != null)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
                    if (!directoryInfo.Exists)
                    {
                        contents = null;
                        return false;
                    }
                    FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
                    IFileInfo[] fileInfoArray = new IFileInfo[fileSystemInfos.Length];
                    for (int index = 0; index != fileSystemInfos.Length; ++index)
                    {
                        FileInfo info = fileSystemInfos[index] as FileInfo;
                        fileInfoArray[index] = info == null ? new PhysicalDirectoryInfo((DirectoryInfo)fileSystemInfos[index]) : (IFileInfo)new PhysicalFileInfo(info);
                    }
                    contents = fileInfoArray;
                    return true;
                }
            }
            catch (ArgumentException ex)
            {
            }
            catch (DirectoryNotFoundException ex)
            {
            }
            catch (IOException ex)
            {
            }
            contents = null;
            return false;
        }

        private bool IsRestricted(FileInfo fileInfo)
        {
            string withoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
            return RestrictedFileNames.ContainsKey(withoutExtension);
        }

        private class PhysicalFileInfo : IFileInfo
        {
            private readonly FileInfo _info;

            public long Length => _info.Length;

            public string PhysicalPath => _info.FullName;

            public string Name => _info.Name;

            public DateTime LastModified => _info.LastWriteTime;

            public bool IsDirectory => false;

            public PhysicalFileInfo(FileInfo info)
            {
                _info = info;
            }

            public Stream CreateReadStream()
            {
                return new FileStream(PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536, FileOptions.Asynchronous | FileOptions.SequentialScan);
            }
        }

        private class PhysicalDirectoryInfo : IFileInfo
        {
            private readonly DirectoryInfo _info;

            public long Length => -1L;

            public string PhysicalPath => _info.FullName;

            public string Name => _info.Name;

            public DateTime LastModified => _info.LastWriteTime;

            public bool IsDirectory => true;

            public PhysicalDirectoryInfo(DirectoryInfo info)
            {
                _info = info;
            }

            public Stream CreateReadStream()
            {
                return null;
            }
        }
    }
}