using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Nancy.Bootstrapper;
using Nancy.Hosting.Self;
using NuFridge.Shared.Logging;
using Owin;

namespace NuFridge.Shared.Server.Web.Nancy
{
    [Serializable]
    public class CustomNancyHost : IDisposable
    {
        private readonly IList<Uri> _baseUriList;
        private readonly HostConfiguration _configuration;
        private readonly INancyBootstrapper _bootstrapper;
        private readonly ILog _log = LogProvider.For<CustomNancyHost>();
        private IDisposable _webApp;

        public AuthenticationSchemeSelector AuthenticationSchemeSelector { get; set; }

        public CustomNancyHost(string url)
        {
            _baseUriList = new[] {new Uri(url)};
        }

        public CustomNancyHost(params Uri[] baseUris)
            : this(NancyBootstrapperLocator.Bootstrapper, new HostConfiguration(), baseUris)
        {
        }

        public CustomNancyHost(HostConfiguration configuration, params Uri[] baseUris)
            : this(NancyBootstrapperLocator.Bootstrapper, configuration, baseUris)
        {
        }

        public CustomNancyHost(INancyBootstrapper bootstrapper, params Uri[] baseUris)
            : this(bootstrapper, new HostConfiguration(), baseUris)
        {
        }

        public CustomNancyHost(INancyBootstrapper bootstrapper, HostConfiguration configuration, params Uri[] baseUris)
        {
            _bootstrapper = bootstrapper;
            _configuration = configuration ?? new HostConfiguration();
            _baseUriList = baseUris;
            bootstrapper.Initialise();
        }

        public CustomNancyHost(Uri baseUri, INancyBootstrapper bootstrapper)
            : this(bootstrapper, new HostConfiguration(), baseUri)
        {
        }

        public CustomNancyHost(Uri baseUri, INancyBootstrapper bootstrapper, HostConfiguration configuration)
            : this(bootstrapper, configuration, baseUri)
        {
        }

        public void Dispose()
        {
            Stop();
            _bootstrapper.Dispose();
        }

        public void Start()
        {
            var urls = GetPrefixes();

            _webApp = WebApp.Start(urls.First(), delegate (IAppBuilder builder)
            {
                builder.MapSignalR("/signalr", new HubConfiguration { EnableDetailedErrors = true, EnableJavaScriptProxies = true, Resolver = GlobalHost.DependencyResolver });
                builder.UseNancy(options => options.Bootstrapper = _bootstrapper);
                builder.UseCors(CorsOptions.AllowAll);
            });
        }

        public void Stop()
        {
            _webApp.Dispose();
        }

        private IEnumerable<string> GetPrefixes()
        {
            foreach (Uri uri in _baseUriList)
            {
                string prefix = uri.ToString();
                if (_configuration.RewriteLocalhost && !uri.Host.Contains("."))
                    prefix = prefix.Replace("localhost", "+");
                yield return prefix;
            }
        }
    }
}