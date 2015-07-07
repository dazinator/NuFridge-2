using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.OData;
using Nancy;
using NuFridge.Shared.Server.Web.OData;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class GetODataRootAction : IAction
    {
        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            NuGetODataModelBuilderODataPackage builder = new NuGetODataModelBuilderODataPackage();
            builder.Build();

            IODataResponseMessage message = new MemoryResponseMessage();
            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings();

            var url = module.Request.Url.ToString();

            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            writerSettings.BaseUri = new Uri(url);


            using (var msgWriter = new ODataMessageWriter(message, writerSettings, builder.Model))
            {
                var workspace = new ODataWorkspace
                {
                    Collections = new List<ODataResourceCollectionInfo>
                        {
                            new ODataResourceCollectionInfo
                            {
                                Name = "Packages",
                                Url = new Uri("Packages", UriKind.Relative)
                            }
                        }
                };

                msgWriter.WriteServiceDocument(workspace);

                var msgStream = message.GetStream();

                msgStream.Seek(0, SeekOrigin.Begin);

                StreamReader reader = new StreamReader(msgStream);

                string text = reader.ReadToEnd();

                return new Response
                {
                    ContentType = "application/xml; charset=utf-8",
                    Contents = contentStream =>
                    {
                        var byteData = Encoding.UTF8.GetBytes(text);
                        contentStream.Write(byteData, 0, byteData.Length);
                        msgStream.Dispose();
                        reader.Dispose();
                    }
                };
            }
        }
    }
}