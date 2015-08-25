using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Application;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Commands
{
    public class InstallCommand : AbstractStandardCommand
    {
        private readonly IWebPortalConfiguration _webPortalConfiguration;
        private readonly ILog _log = LogProvider.For<InstallCommand>();

        public InstallCommand(IApplicationInstanceSelector selector, IWebPortalConfiguration webPortalConfiguration) : base(selector)
        {
            _webPortalConfiguration = webPortalConfiguration;
        }

        protected override void Start()
        {
            base.Start();

            var baseUrl = _webPortalConfiguration.ListenPrefixes;

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            var installUrl = $"{baseUrl}#/setup";

            _log.Info("InstallUrl: " + installUrl);

            System.Diagnostics.Process.Start("iexplore", installUrl);
        }
    }
}