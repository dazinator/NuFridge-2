﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Server.Web.OData;

namespace NuFridge.Shared.Server.Web
{
    public class ODataPackages
    {
        public static Stream CreatePackagesStream(string baseUrl, string baseAddress, IEnumerable<IInternalPackage> packages,int total, string strSelectFields, bool isXmlResponse)
        {
            var writerSettings = new ODataMessageWriterSettings
            {
                Indent = true,
                CheckCharacters = false,
                BaseUri = new Uri(baseUrl),
                Version = ODataVersion.V3
            };

            if (!isXmlResponse)
            {
                writerSettings.SetMetadataDocumentUri(new Uri(baseAddress));
                writerSettings.SetContentType(ODataFormat.VerboseJson);
            }
            else
            {
                writerSettings.SetContentType(ODataFormat.Atom);
            }

            var responseMessage = new MemoryResponseMessage();
            var writer = new ODataMessageWriter(responseMessage, writerSettings);

            var feedWriter = writer.CreateODataFeedWriter();
            feedWriter.WriteStart(new ODataFeed { Id = "Packages", Count = total});


            var pks = packages.Select(pk => new ODataPackage(pk));

            string[] selectFields = strSelectFields.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLower()).ToArray();

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

            var oDataEntry = new ODataEntry
            {
                EditLink = new Uri(baseAddress + entryId, UriKind.Absolute),
                Id = baseAddress + entryId,
                TypeName = "Package",
                MediaResource = new ODataStreamReferenceValue
                {
                    ContentType = "application/zip",
                    ReadLink = new Uri(baseAddress + "package/" + package.Id + "/" + package.Version)
                },
                Properties = GetProperties(package, properties)
            };

            var atom = oDataEntry.Atom();
            atom.Title = package.Id;

            List<AtomPersonMetadata> authors = new List<AtomPersonMetadata>();
            authors.AddRange(package.Authors.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => new AtomPersonMetadata {Name = s}).ToArray());
            atom.Authors = authors;

            atom.Summary = package.Summary;

            return oDataEntry;
        }



        private static IEnumerable<ODataProperty> GetProperties(object obj, string[] propertiesToInclude)
        {
            var properties =
                obj.GetType()
                    .GetProperties()
                    .Select(property => new ODataProperty {Name = property.Name, Value = property.GetValue(obj)})
                    .ToArray();

            if (propertiesToInclude.Any())
            {
                return properties.Where(pr => propertiesToInclude.Contains(pr.Name.ToLower()));
            }

            return properties;
        }
    }
}
