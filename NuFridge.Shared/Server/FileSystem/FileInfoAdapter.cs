using System;
using System.IO;

namespace NuFridge.Shared.Server.FileSystem
{
    public class FileInfoAdapter : IFileInfo
    {
        private readonly FileInfo _info;

        public string FullPath
        {
            get
            {
                return _info.FullName;
            }
        }

        public string Extension
        {
            get
            {
                return _info.Extension;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                return _info.LastAccessTimeUtc;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                return _info.LastWriteTimeUtc;
            }
        }

        public FileInfoAdapter(FileInfo info)
        {
            _info = info;
        }
    }
}
