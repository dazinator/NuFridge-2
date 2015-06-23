using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.OData;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet;
using NuGet;

namespace NuFridge.Shared.Server.Web
{
    public class ODataPackages
    {
        public static Stream CreatePackagesStream(string baseUrl, IInternalPackageRepository packageRepository, string baseAddress, IEnumerable<IPackage> packages, int feedId, int skip, int take)
        {
            var writerSettings = new ODataMessageWriterSettings()
            {
                Indent = true, // pretty printing
                CheckCharacters = false,
                BaseUri = new Uri(baseUrl),
                Version = ODataVersion.V3
            };

            writerSettings.SetContentType(ODataFormat.Atom);

            var responseMessage = new MemoryResponseMessage();
            var writer = new ODataMessageWriter(responseMessage, writerSettings);

            var feedWriter = writer.CreateODataFeedWriter();
            feedWriter.WriteStart(new ODataFeed() { Id = "Packages", Count = packages.Count()});

            var webPackages = packages.Skip(skip).Take(take).Select(pk => new WebZipPackage(pk, new Uri(string.Format("{0}/package/{1}/{2}", baseUrl, pk.Id, pk.Version)))).ToList();

            foreach (var package in webPackages)
            {
                feedWriter.WriteStart(MapPackageToEntry(baseAddress, package));
                feedWriter.WriteEnd();
            }

            feedWriter.WriteEnd();
            feedWriter.Flush();

            var stream = responseMessage.GetStream();
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        private static ODataEntry MapPackageToEntry(string baseAddress, IWebPackage package)
        {
            var dtoPackage = MapPackageToDto(package);
            var entryId = "Packages(Id='" + package.Id + "',Version='" + package.Version + "')";

            var oDataEntry = new ODataEntry()
            {
                EditLink = new Uri(baseAddress + entryId, UriKind.Absolute),
                Id = baseAddress + entryId,
                TypeName = "Package",
                MediaResource = new ODataStreamReferenceValue()
                {
                    ContentType = "application/zip",
                    ReadLink = package.Uri,
                },

                Properties = GetProperties(dtoPackage)
            };

            return oDataEntry;
        }

        private static WebPackage MapPackageToDto(IWebPackage package)
        {
            var tempPackage = new WebPackage()
            {
                Id = package.Id,
                Authors = string.Join(", ", package.Authors),
                Copyright = package.Copyright,
                Owners = string.Join(", ", package.Owners),
                ProjectUrl = package.ProjectUrl != null ? package.ProjectUrl.ToString() : null,
                Title = package.Title,
                Version = package.Version != null ? package.Version.ToString() : null,
                IsPrerelease = false,
                DownloadCount = package.DownloadCount,
                RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                DevelopmentDependency = package.DevelopmentDependency,
                Description = package.Description,
                Published = package.Published != null ? package.Published.Value.UtcDateTime : new DateTime(2014, 1, 1),
                LastUpdated = new DateTime(2014, 1, 1),
                PackageHash = package.GetHash("SHA512"),
                PackageHashAlgorithm = "SHA512",
                PackageSize = package.GetStream().Length,
                IsAbsoluteLatestVersion = package.IsAbsoluteLatestVersion,
                IsLatestVersion = package.IsLatestVersion,
                Listed = package.Listed,
                VersionDownloadCount = package.DownloadCount
            };
            return tempPackage;
        }

        private static IEnumerable<ODataProperty> GetProperties(object obj)
        {
            return obj.GetType().GetProperties().Select(property => new ODataProperty() { Name = property.Name, Value = property.GetValue(obj) }).ToArray();
        }

        public static Stream CreatePackageStream(string baseAddress, IWebPackage package)
        {
            var writerSettings = new ODataMessageWriterSettings()
            {
                Indent = true, // pretty printing
                CheckCharacters = false,
                BaseUri = new Uri("http://localhost:12345"),
                Version = ODataVersion.V3
            };

            writerSettings.SetContentType(ODataFormat.Atom);

            var responseMessage = new MemoryResponseMessage();
            var writer = new ODataMessageWriter(responseMessage, writerSettings);

            var feedWriter = writer.CreateODataEntryWriter();
            feedWriter.WriteStart(MapPackageToEntry(baseAddress, package));
            feedWriter.WriteEnd();
            feedWriter.Flush();

            var stream = responseMessage.GetStream();
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
