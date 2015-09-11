using System;
using System.IO;

namespace NuFridge.Shared.FileSystem
{
    public class FileInfoAdapter : IFileInfo
    {
        private readonly FileInfo _info;

        public string FullPath => _info.FullName;

        public string Extension => _info.Extension;

        public DateTime LastAccessTimeUtc => _info.LastAccessTimeUtc;

        public DateTime LastWriteTimeUtc => _info.LastWriteTimeUtc;

        public FileInfoAdapter(FileInfo info)
        {
            _info = info;
        }
    }
}