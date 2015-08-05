using System;
using System.Linq;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Logging;
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
            using (var dbContext = new DatabaseContext())
            {
                feed =
                    dbContext.Feeds.AsNoTracking()
                        .FirstOrDefault(f => f.Name.Equals(feedName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (feed == null)
            {
                _log.Warn("Feed does not exist called " + feedName);
                var errorResponse = module.Response.AsText("Feed does not exist.");
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                return errorResponse;
            }

            using (var dbContext = new DatabaseContext())
            {
                config = dbContext.FeedConfigurations.AsNoTracking().FirstOrDefault(fc => fc.FeedId == feed.Id);
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