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
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using NuGet;

namespace NuFridge.Shared.Server.NuGet.FastZipPackage
{
    public class FastZipPackage : FastZipPackageBase, IFastZipPackage, IPackage, IPackageMetadata, IPackageName, IServerPackageMetadata, IDisposable
    {
        private static readonly ISet<string> PackageFileExcludeExtensions = new HashSet<string>
        {
            ".pdscmp",
            ".psmdcp",
            ".nuspec",
            ".rels"
        };
        private readonly Lazy<IList<FrameworkName>> _supportedFrameworks;

        public string Id { get; set; }

        public SemanticVersion Version { get; set; }

        public string FileLocation { get; set; }

        public string Title { get; private set; }

        public IEnumerable<string> Authors { get; private set; }

        public IEnumerable<string> Owners { get; private set; }

        public Uri IconUrl { get; private set; }

        public Uri LicenseUrl { get; private set; }

        public Uri ProjectUrl { get; private set; }

        public bool RequireLicenseAcceptance { get; private set; }

        public string Description { get; private set; }

        public string Summary { get; private set; }

        public string ReleaseNotes { get; private set; }

        public string Language { get; private set; }

        public string Tags { get; private set; }

        public string Copyright { get; private set; }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies { get; set; }

        public IEnumerable<PackageDependencySet> DependencySets { get; private set; }

        public Uri ReportAbuseUrl
        {
            get
            {
                return null;
            }
        }

        public int DownloadCount
        {
            get
            {
                return 0;
            }
        }

        public byte[] Hash { get; set; }

        public Version MinClientVersion { get; private set; }

        public bool DevelopmentDependency { get; private set; }

        public IEnumerable<IPackageFile> Files { get; set; }

        public void ExtractContents(IFileSystem fileSystem, string extractPath)
        {
            foreach (PhysicalPackageFile physicalPackageFile in GetFiles().Cast<PhysicalPackageFile>())
            {
                string path = Path.Combine(extractPath, physicalPackageFile.TargetPath);
                using (Stream stream = physicalPackageFile.GetStream())
                    fileSystem.AddFile(path, stream);
            }
        }

        public bool IsAbsoluteLatestVersion
        {
            get
            {
                return false;
            }
        }

        public bool IsLatestVersion
        {
            get
            {
                return false;
            }
        }

        public bool Listed { get; set; }

        public DateTimeOffset? Published
        {
            get
            {
                return new DateTimeOffset?();
            }
        }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences
        {
            get
            {
                return Enumerable.Empty<IPackageAssemblyReference>();
            }
        }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences { get; private set; }

        public DateTimeOffset Created { get; private set; }

        public long Size { get; private set; }

        protected internal FastZipPackage()
        {
            FrameworkAssemblies = Enumerable.Empty<FrameworkAssemblyReference>();
            DependencySets = Enumerable.Empty<PackageDependencySet>();
            Files = Enumerable.Empty<IPackageFile>();
            _supportedFrameworks = new Lazy<IList<FrameworkName>>(ComputeSupportedFrameworks, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static FastZipPackage Open(string fileLocation, Stream stream, byte[] hash)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream.CanRead must be supported.", "stream");
            if (!stream.CanSeek)
                throw new ArgumentException("Stream.CanSeek must be supported.", "stream");
            FastZipPackage fastZipPackage = new FastZipPackage
            {
                FileLocation = fileLocation
            };
            using (Package package = Package.Open(stream, FileMode.Open, FileAccess.Read))
            {
                fastZipPackage.ProcessManifest(package);
                fastZipPackage.ProcessPackageContents(package);
            }
            stream.Seek(0L, SeekOrigin.Begin);
            fastZipPackage.ProcessFileMetadata(stream);
            fastZipPackage.Size = stream.Length;
            fastZipPackage.Hash = hash;
            return fastZipPackage;
        }

        public static FastZipPackage Open(string fileLocation, byte[] hash)
        {
            using (FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
                return Open(fileLocation, fileStream, hash);
        }

        public static IFastZipPackage Open(string fileLocation, IHashProvider hashProvider)
        {
            using (FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
            {
                byte[] hash = hashProvider.CalculateHash(fileStream);
                fileStream.Seek(0L, SeekOrigin.Begin);
                return Open(fileLocation, fileStream, hash);
            }
        }

        public override Stream GetStream()
        {
            return new FileStream(FileLocation, FileMode.Open, FileAccess.Read);
        }

        public string GetFileLocation()
        {
            return FileLocation;
        }

        public IEnumerable<IPackageFile> GetFiles()
        {
            return Files;
        }

        public IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            return _supportedFrameworks.Value;
        }

        public IList<FrameworkName> ComputeSupportedFrameworks()
        {
            return FrameworkAssemblies.SelectMany(f => f.SupportedFrameworks).Union(Files.SelectMany(f => f.SupportedFrameworks)).Where(f =>
            {
                if (f != null)
                    return f != VersionUtility.UnsupportedFrameworkName;
                return false;
            }).ToArray();
        }

        protected virtual void ProcessPackageContents(Package package)
        {
            Files = package.GetParts().Where(p => !PackageFileExcludeExtensions.Contains(Path.GetExtension(p.Uri.OriginalString))).Select(p => new FastZipPackageFile(this, p.Uri.OriginalString)).ToArray();
        }

        protected virtual void ProcessFileMetadata(Stream stream)
        {
            Created = GetPackageCreatedDateTime(stream);
        }

        protected virtual void ProcessManifest(Package package)
        {
            PackageRelationship packageRelationship = package.GetRelationshipsByType("http://schemas.microsoft.com/packaging/2010/07/manifest").SingleOrDefault();
            if (packageRelationship == null)
                throw new InvalidOperationException("Package does not contain a manifest");
            ProcessManifest(package.GetPart(packageRelationship.TargetUri).GetStream());
        }

        protected virtual void ProcessManifest(Stream manifestStream)
        {
            IPackageMetadata packageMetadata = Manifest.ReadFrom(manifestStream, false).Metadata;
            Id = packageMetadata.Id;
            Version = packageMetadata.Version;
            MinClientVersion = packageMetadata.MinClientVersion;
            Title = packageMetadata.Title;
            Authors = packageMetadata.Authors;
            Owners = packageMetadata.Owners;
            IconUrl = packageMetadata.IconUrl;
            LicenseUrl = packageMetadata.LicenseUrl;
            ProjectUrl = packageMetadata.ProjectUrl;
            RequireLicenseAcceptance = packageMetadata.RequireLicenseAcceptance;
            Description = packageMetadata.Description;
            Summary = packageMetadata.Summary;
            ReleaseNotes = packageMetadata.ReleaseNotes;
            Language = packageMetadata.Language;
            Tags = packageMetadata.Tags;
            DependencySets = packageMetadata.DependencySets;
            FrameworkAssemblies = packageMetadata.FrameworkAssemblies;
            Copyright = packageMetadata.Copyright;
            PackageAssemblyReferences = packageMetadata.PackageAssemblyReferences;
            DevelopmentDependency = packageMetadata.DevelopmentDependency;
            if (string.IsNullOrEmpty(Tags))
                return;
            Tags = " " + Tags + " ";
        }
    }
}
