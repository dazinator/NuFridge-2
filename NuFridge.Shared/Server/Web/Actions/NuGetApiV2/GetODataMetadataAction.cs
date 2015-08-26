﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.OData;
using Nancy;
using Nancy.Responses.Negotiation;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Server.Web.OData;

namespace NuFridge.Shared.Server.Web.Actions.NuGetApiV2
{
    public class GetODataMetadataAction : IAction
    {
        private readonly IFeedService _feedService;

        public GetODataMetadataAction(IFeedService feedService)
        {
            _feedService = feedService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;

            Feed feed = _feedService.Find(feedName, true);

            if (feed == null)
            {
                var response = module.Response.AsText($"Feed does not exist called {feedName}.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            NuGetODataModelBuilderODataPackage builder = new NuGetODataModelBuilderODataPackage();
            builder.Build();

            IODataResponseMessage message = new MemoryResponseMessage();
            ODataMessageWriterSettings writerSettings = new ODataMessageWriterSettings();

            var enumerable = module.Request.Headers.Accept;
            var ranges = enumerable.OrderByDescending(o => o.Item2).Select(o => new MediaRange(o.Item1)).ToList();

            bool isXmlResponse = false;

            foreach (var mediaRange in ranges)
            {
                if (mediaRange.Matches("application/xml") || mediaRange.Matches("application/atom+xml;type=feed") || mediaRange.Matches("application/atom+xml"))
                {
                    isXmlResponse = true;
                }
            }

            if (!isXmlResponse)
            {
                return module.Negotiate.WithStatusCode(HttpStatusCode.BadRequest).WithContentType("Unsupported media type requested.");
            }
            else
            {
                writerSettings.SetContentType(ODataFormat.Atom);
            }

            using (var msgWriter = new ODataMessageWriter(message, writerSettings, builder.Model))
            {
                msgWriter.WriteMetadataDocument();
                
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