using Nancy;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Model;
using NuFridge.Shared.Model.Interfaces;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.SymbolsApi
{
    class GetSourceFilesAction : IAction
    {
        private readonly IStore _store;
        private readonly SymbolSource _symbolSource;
        private readonly ILog _log = LogProvider.For<GetSourceFilesAction>();

        public GetSourceFilesAction(IStore store, SymbolSource symbolSource)
        {
            _store = store;
            _symbolSource = symbolSource;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            string id = parameters.id;
            string version = parameters.version;
            string path = parameters.path;

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

            var fileStream = _symbolSource.OpenPackageSourceFile(config, id, version, path);

            if (fileStream == null)
            {
                return new Response { StatusCode = HttpStatusCode.NotFound };
            }

            return new Response { StatusCode = HttpStatusCode.OK, Contents = stream => fileStream.CopyTo(stream) };
        }
    }
}
