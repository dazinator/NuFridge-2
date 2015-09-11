using System;

namespace NuFridge.Shared.FileSystem
{
    public interface IFileInfo
    {
        string FullPath { get; }

        string Extension { get; }

        DateTime LastAccessTimeUtc { get; }

        DateTime LastWriteTimeUtc { get; }
    }
}
