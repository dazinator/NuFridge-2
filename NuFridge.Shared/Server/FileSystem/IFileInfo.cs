using System;

namespace NuFridge.Shared.Server.FileSystem
{
    public interface IFileInfo
    {
        string FullPath { get; }

        string Extension { get; }

        DateTime LastAccessTimeUtc { get; }

        DateTime LastWriteTimeUtc { get; }
    }
}
