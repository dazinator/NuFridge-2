using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace NuFridge.Service.Model.Dto
{
    public class DtoPackage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Authors { get; set; }
        public string IconUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string Owners { get; set; }
        public string ProjectUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public string Tags { get; set; }
        public string Version { get; set; }
        public string Published { get; set; }

        //Does not include pre-release packages
        public bool IsLatestVersion { get; set; }

        //Includes pre-release packages
        public bool IsAbsoluteLatestVersion { get; set; }
    }
}
