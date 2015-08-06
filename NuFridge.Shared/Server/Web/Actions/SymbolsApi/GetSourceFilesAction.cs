using System;
using System.Linq;
using Nancy;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Model.Interfaces;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.NuGet.Symbols;
using NuFridge.Shared.Server.Storage;

namespace NuFridge.Shared.Server.Web.Actions.SymbolsApi
{
    class GetSourceFilesAction : IAction
    {
        private readonly SymbolSource _symbolSource;
        private readonly IFeedService _feedService;
        private readonly IFeedConfigurationService _feedConfigurationService;
        private readonly ILog _log = LogProvider.For<GetSourceFilesAction>();

        public GetSourceFilesAction(SymbolSource symbolSource, IFeedService feedService, IFeedConfigurationService feedConfigurationService)
        {
            _symbolSource = symbolSource;
            _feedService = feedService;
            _feedConfigurationService = feedConfigurationService;
        }

        public dynamic Execute(dynamic parameters, INancyModule module)
        {
            string feedName = parameters.feed;
            string id = parameters.id;
            string version = parameters.version;
            string path = parameters.path;

            IFeed feed = _feedService.Find(feedName, false);
            

            if (feed == null)
            {
                _log.Warn($"Feed does not exist called {feedName}");
                var errorResponse = module.Response.AsText("Feed does not exist.");
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                return errorResponse;
            }

            IFeedConfiguration config = _feedConfigurationService.FindByFeedId(feed.Id);

            if (config == null)
            {
                _log.Warn($"Feed config does not exist called {feedName}");
                var errorResponse = module.Response.AsText("Feed config does not exist.");
                errorResponse.StatusCode = HttpStatusCode.BadRequest;
                return errorResponse;
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
