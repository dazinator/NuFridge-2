/* 
* @author themotleyfool https://github.com/themotleyfool/NuGet.Lucene
* Apache License
* Version 2.0, January 2004
* http://www.apache.org/licenses/
*
* Copyright 2008-2012 by themotleyfool
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.NuGet.FastZipPackage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet.Symbols
{
    public class SymbolSource : ISymbolSource
    {
        private readonly ILocalFileSystem _localFileSystem;
        private readonly SymbolTools _symbolTools;

        public SymbolSource(ILocalFileSystem localFileSystem, SymbolTools symbolTools)
        {
            _localFileSystem = localFileSystem;
            _symbolTools = symbolTools;
        }

        public bool KeepSourcesCompressed { get; set; }
        public bool Enabled => _symbolTools.Enabled;

        public void AddSymbolPackage(IFeedConfiguration config, IPackage package, string symbolSourceUri)
        {
            try
            {
                CopyNupkgToTargetPath(config.SymbolsDirectory, package);
                ProcessSymbols(config.SymbolsDirectory, package, symbolSourceUri);
            }
            finally
            {
                var disposablePackage = package as IDisposable;
                disposablePackage?.Dispose();
            }
        }

        protected void CopyNupkgToTargetPath(string symbolsDirectory, IPackage package)
        {
            var targetFile = new FileInfo(GetNupkgPath(symbolsDirectory, package.Id, package.Version.ToString()));

            if (targetFile.Directory != null && !targetFile.Directory.Exists)
            {
                targetFile.Directory.Create();
            }
            else if (targetFile.Exists)
            {
                targetFile.Delete();
            }

            var fastZipPackage = package as FastZipPackage.FastZipPackage;
            if (fastZipPackage != null)
            {
                File.Move(fastZipPackage.GetFileLocation(), targetFile.FullName);
                fastZipPackage.FileLocation = targetFile.FullName;
                return;
            }

            using (var sourceStream = package.GetStream())
            {
                using (var targetStream = targetFile.Open(FileMode.Create, FileAccess.Write))
                {
                    sourceStream.CopyTo(targetStream);
                }
            }
        }

        public void RemoveSymbolPackage(string symbolsDirectory, string packageId, string packageVersion)
        {
            var nupkgPath = GetNupkgPath(symbolsDirectory, packageId, packageVersion);
            if (File.Exists(nupkgPath))
            {
                File.Delete(nupkgPath);
            }

            var folderPath = GetUnzippedPackagePath(symbolsDirectory, packageId, packageVersion);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }

        public Stream OpenFile(IFeedConfiguration config, string packageId, string packageVersion, string relativePath)
        {
            var parts = new[]
{
                        packageId,
                        packageVersion,
                        relativePath
                    };

            var fullPath = Path.Combine(config.SymbolsDirectory, Path.Combine(parts).Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(fullPath))
            {
                if (!string.IsNullOrWhiteSpace(packageId) && !string.IsNullOrWhiteSpace(packageVersion))
                {
                    parts = new[]
                    {
                        packageId,
                        packageVersion,
                        "src",
                        relativePath
                    };

                    var srcFullPath = Path.Combine(config.SymbolsDirectory, Path.Combine(parts).Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(srcFullPath))
                    {
                        return File.Open(srcFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
                return null;

            }

            return File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Stream OpenPackageSourceFile(IFeedConfiguration config, string packageId, string packageVersion, string relativePath)
        {
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);


            var stream = OpenFile(config, packageId, packageVersion, relativePath);

            if (stream != null) return stream;

            var packagePath = GetNupkgPath(config.SymbolsDirectory, packageId, packageVersion);
            if (!File.Exists(packagePath)) return null;

            var srcPath = Path.Combine("src", relativePath);

            var packageFile = FastZipPackage.FastZipPackage.Open(packagePath, new byte[0]);

            var file = packageFile.GetFiles().SingleOrDefault(f => f.Path.Equals(srcPath, StringComparison.InvariantCultureIgnoreCase));
            return file != null ? new PackageDisposingStream(packageFile, file.GetStream()) : null;
        }

        public void UnzipPackage(string symbolsDirectory, IPackage package)
        {
            var dir = GetUnzippedPackagePath(symbolsDirectory, package.Id, package.Version.ToString());

            Directory.CreateDirectory(dir);
            var visitedDirectories = new HashSet<string>();

            foreach (var file in package.GetFiles("src"))
            {
                var filePath = Path.Combine(dir, file.Path);
                var fileDir = Path.GetDirectoryName(filePath);

                if (fileDir != null && !visitedDirectories.Contains(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                    visitedDirectories.Add(fileDir);
                }

                using (var writeStream = File.OpenWrite(filePath))
                {
                    using (var readStream = file.GetStream())
                    {
                        readStream.CopyTo(writeStream);
                    }
                }
            }
        }

        public void ProcessSymbols(string symbolsDirectory, IPackage package, string symbolSourceUri)
        {
            var files = package.GetLibFiles().Where(f => f.Path.EndsWith("pdb", StringComparison.InvariantCultureIgnoreCase));

            var tempFolder = _localFileSystem.CreateTemporaryDirectory();

            foreach (var file in files)
            {
                var filePath = Path.Combine(tempFolder, file.Path);
                var fileDir = Path.GetDirectoryName(filePath);

                if (fileDir != null && !Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }

                using (var writeStream = File.Open(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (var readStream = file.GetStream())
                    {
                        readStream.CopyTo(writeStream);
                    }
                }

                ProcessSymbolFile(package, symbolsDirectory, filePath, symbolSourceUri);
            }

            _localFileSystem.DeleteDirectory(tempFolder);
        }

        public void ProcessSymbolFile(IPackage package, string symbolsDirectory, string symbolFilePath, string symbolSourceUri)
        {
            var referencedSources = (_symbolTools.GetSources(symbolFilePath)).ToList();

            var sourceFiles = new HashSet<string>(package.GetFiles("src").Select(f => f.Path.Substring(4)));

            if (referencedSources.Any() && sourceFiles.Any())
            {
                var sourceMapper = new SymbolSourceMapper();
                var mappings = sourceMapper.CreateSourceMappingIndex(package, symbolSourceUri, referencedSources, sourceFiles);

                _symbolTools.MapSources(symbolFilePath, mappings);
                _symbolTools.IndexSymbolFile(package, symbolsDirectory, symbolFilePath);
            }
        }

        public virtual string GetNupkgPath(string symbolsDirectory, string packageId, string packageVersion)
        {
            return Path.Combine(symbolsDirectory, packageId, packageId + "." + packageVersion + ".symbols.nupkg");
        }

        public virtual string GetUnzippedPackagePath(string symbolsDirectory, string packageId, string packageVersion)
        {
            return Path.Combine(symbolsDirectory, packageId, packageVersion);
        }

        protected virtual string GetTempFolderPathForPackage(string symbolsDirectory, IPackage package)
        {
            return Path.Combine(symbolsDirectory, package.Id + "-" + package.Version + ".tmp");
        }

        class PackageDisposingStream : ReadStream
        {
            private readonly IFastZipPackage package;

            public PackageDisposingStream(IFastZipPackage package, Stream stream) : base(stream)
            {
                this.package = package;
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                package.Dispose();
            }
        }
    }

    public interface ISymbolSource
    {
        void AddSymbolPackage(IFeedConfiguration config, IPackage package, string symbolSourceUri);
        void RemoveSymbolPackage(string symbolsDirectory, string packageId, string packageVersion);
        Stream OpenFile(IFeedConfiguration config, string packageId, string packageVersion, string relativePath);
        bool Enabled { get; }
        Stream OpenPackageSourceFile(IFeedConfiguration config, string packageId, string packageVersion,
            string relativePath);
    }
}