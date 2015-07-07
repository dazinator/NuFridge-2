﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Server.Web
{
    public class WebPackage
    {
        public string Authors { get; set; }

        public string Copyright { get; set; }

        public string Dependencies { get; set; }

        public string Description { get; set; }

        public bool DevelopmentDependency { get; set; }

        public int DownloadCount { get; set; }

        public string IconUrl { get; set; }

        public string Id { get; set; }

        public bool IsAbsoluteLatestVersion { get; set; }

        public bool IsLatestVersion { get; set; }

        public bool IsPrerelease { get; set; }

        public DateTime LastUpdated { get; set; }

        public string LicenseUrl { get; set; }

        public bool Listed { get; set; }

        public string MinClientVersion { get; set; }

        public string Owners { get; set; }

        public string PackageHash { get; set; }

        public string PackageHashAlgorithm { get; set; }

        public long PackageSize { get; set; }

        public string ProjectUrl { get; set; }

        public DateTime Published { get; set; }

        public string ReleaseNotes { get; set; }

        public bool RequireLicenseAcceptance { get; set; }

        public string Summary { get; set; }

        public string Tags { get; set; }

        public string Title { get; set; }

        public string Version { get; set; }

        public int VersionDownloadCount { get; set; }
    }
}