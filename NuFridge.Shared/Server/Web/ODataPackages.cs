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
        public static Stream CreatePackagesStream(string baseUrl, IInternalPackageRepository packageRepository, string baseAddress, IEnumerable<IInternalPackage> packages, int feedId, int total, string strSelectFields)
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

            string[] selectFields = strSelectFields.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLower()).ToArray();

            if (selectFields.Contains("id"))
            {
                selectFields[Array.IndexOf(selectFields, "id")] = "packageid";
            }

            foreach (var package in pks)
            {
                feedWriter.WriteStart(MapPackageToEntry(baseAddress, package, selectFields));
                feedWriter.WriteEnd();
            }

            feedWriter.WriteEnd();
            feedWriter.Flush();

            var stream = responseMessage.GetStream();
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        internal static ODataEntry MapPackageToEntry(string baseAddress, ODataPackage package, string[] properties)
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
                    ReadLink = new Uri(baseAddress + "package/" + package.Id + "/" + package.Version),
                },
                Properties = GetProperties(package, properties)
            };

            return oDataEntry;
        }



        private static IEnumerable<ODataProperty> GetProperties(object obj, string[] propertiesToInclude)
        {
            var properties =
                obj.GetType()
                    .GetProperties()
                    .Select(property => new ODataProperty() {Name = property.Name, Value = property.GetValue(obj)})
                    .ToArray();

            if (propertiesToInclude.Any())
            {
                return properties.Where(pr => propertiesToInclude.Contains(pr.Name.ToLower()));
            }

            return properties;
        }
    }
}
