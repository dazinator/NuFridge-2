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
using NuFridge.Shared.Server.Web.OData;
using NuGet;

namespace NuFridge.Shared.Server.Web
{
    public class ODataPackages
    {
        public static Stream CreatePackagesStream(string baseUrl, IInternalPackageRepository packageRepository, string baseAddress, IEnumerable<InternalPackage> packages, int feedId, int total)
        {
            var writerSettings = new ODataMessageWriterSettings()
            {
                Indent = true,
                CheckCharacters = false,
                BaseUri = new Uri(baseUrl),
                Version = ODataVersion.V3
            };

            writerSettings.SetContentType(ODataFormat.Atom);

            var responseMessage = new MemoryResponseMessage();
            var writer = new ODataMessageWriter(responseMessage, writerSettings);

            var feedWriter = writer.CreateODataFeedWriter();
            feedWriter.WriteStart(new ODataFeed() { Id = "Packages", Count = total});


            var pks = packages.Select(pk => new ODataPackage(pk));

            foreach (var package in pks)
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

        private static ODataEntry MapPackageToEntry(string baseAddress, ODataPackage package)
        {

            var entryId = "Packages(Id='" + package.Id + "',Version='" + package.Version + "')";

            var oDataEntry = new ODataEntry()
            {
                EditLink = new Uri(baseAddress + entryId, UriKind.Absolute),
                Id = baseAddress + entryId,
                TypeName = "Package",
                MediaResource = new ODataStreamReferenceValue()
                {
                    ContentType = "application/zip",
                    ReadLink = new Uri(baseAddress + "/package/" + package.Id + "/" + package.Version),
                },

                Properties = GetProperties(package)
            };

            return oDataEntry;
        }



        private static IEnumerable<ODataProperty> GetProperties(object obj)
        {
            return obj.GetType().GetProperties().Select(property => new ODataProperty() { Name = property.Name, Value = property.GetValue(obj) }).ToArray();
        }

        public static Stream CreatePackageStream(string baseAddress, ODataPackage package)
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
