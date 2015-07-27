using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Responses;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.SymbolsApi
{
    class GetSymbolFileAction : IAction
    {
        private readonly IStore _store;
        private readonly SymbolSource _source;
        private readonly ILog _log = LogProvider.For<GetSymbolFileAction>();

        public GetSymbolFileAction(IStore store, SymbolSource source)
        {
            _store = store;
            _source = source;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            string path = parameters.path;

            if (string.IsNullOrEmpty(path))
            {
                return new Response { StatusCode = HttpStatusCode.NoContent };
            }

            IFeed feed;
            IFeedConfiguration config;
            using (var transaction = _store.BeginTransaction())
            {
                feed = transaction.Query<IFeed>()
                    .Where("Name = @name")
                    .Parameter("name", feedName)
                    .First();
            }

            if (feed == null)
            {
                _log.Warn("Feed does not exist called " + feedName);
                var errorResponse = module.Response.AsText("Feed does not exist.");
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                return errorResponse;
            }

            using (var transaction = _store.BeginTransaction())
            {
                config =
                    transaction.Query<IFeedConfiguration>()
                        .Where("FeedId = @feedId")
                        .Parameter("feedId", feed.Id)
                        .First();
            }

            var fileStream = _source.OpenFile(config, null, null, path);

            if (fileStream == null)
            {
                return new Response { StatusCode = HttpStatusCode.NotFound };
            }

            return new Response { StatusCode = HttpStatusCode.OK, Contents = stream => fileStream.CopyTo(stream) };
        }
    }
}